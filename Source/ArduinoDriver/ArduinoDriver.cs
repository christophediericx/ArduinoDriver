using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using ArduinoDriver.SerialEngines;
using ArduinoDriver.SerialProtocol;
using ArduinoUploader;
using ArduinoUploader.Hardware;
using NLog;
using RJCP.IO.Ports;

namespace ArduinoDriver
{
    /// <summary>
    /// An ArduinoDriver can be used to communicate with an attached Arduino by way of sending requests and receiving responses.
    /// These messages are sent over a live serial connection (via a serial protocol) to the Arduino. The required listener can be 
    /// automatically deployed to the Arduino.
    /// </summary>
    public class ArduinoDriver : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IList<ArduinoModel> alwaysRedeployListeners = 
            new List<ArduinoModel> { ArduinoModel.NanoR3 };
        private const int defaultRebootGraceTime = 2000;
        private readonly IDictionary<ArduinoModel, int> rebootGraceTimes = 
            new Dictionary<ArduinoModel, int>
            {
                {ArduinoModel.Micro, 8000},
                {ArduinoModel.Mega2560, 4000}
            };
        private const int CurrentProtocolMajorVersion = 1;
        private const int CurrentProtocolMinorVersion = 0;
        private const int DriverBaudRate = 115200;
        private ArduinoDriverSerialPort port;
        private ArduinoDriverConfiguration config;
        private Func<ISerialPortEngine> engineFunc;

        private const string ArduinoListenerHexResourceFileName =
            "ArduinoDriver.ArduinoListener.ArduinoListener.ino.{0}.hex";

        /// <summary>
        /// Creates a new ArduinoDriver instance. The relevant portname will be autodetected if possible.
        /// </summary>
        /// <param name="arduinoModel"></param>
        /// <param name="autoBootstrap"></param>
        public ArduinoDriver(ArduinoModel arduinoModel, bool autoBootstrap = false)
        {
            logger.Info(
                "Instantiating ArduinoDriver (model {0}) with autoconfiguration of port name...",
                arduinoModel);

            var possiblePortNames = SerialPortStream.GetPortNames().Distinct();
            string unambiguousPortName = null;
            try
            {
                unambiguousPortName = possiblePortNames.SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                // More than one posible hit.
            }
            if (unambiguousPortName == null)
                throw new IOException(
                    "Unable to autoconfigure ArduinoDriver port name, since there is not exactly a single "
                    + "COM port available. Please use the ArduinoDriver with the named port constructor!");

            Initialize(new ArduinoDriverConfiguration
            {
                ArduinoModel = arduinoModel, 
                PortName = unambiguousPortName, 
                AutoBootstrap = autoBootstrap
            });
        }

        /// <summary>
        /// Creates a new ArduinoDriver instance for a specified portName.
        /// </summary>
        /// <param name="arduinoModel"></param>
        /// <param name="portName">The COM portname to create the ArduinoDriver instance for.</param>
        /// <param name="autoBootStrap">Determines if an listener is automatically deployed to the Arduino if required.</param>
        public ArduinoDriver(ArduinoModel arduinoModel, string portName, bool autoBootstrap = false)
        {
            Initialize(new ArduinoDriverConfiguration
            {
                ArduinoModel = arduinoModel,
                PortName = portName,
                AutoBootstrap = autoBootstrap
            });
        }

        /// <summary>
        /// Sends an ArduinoRequest to the Arduino, and returns the corresponding ArduinoResponse object.
        /// </summary>
        /// <param name="request">The ArduinoRequest request object to send.</param>
        /// <returns>The ArduinoResponse response object.</returns>
        public ArduinoResponse Send(ArduinoRequest request)
        {
            return port.Send(request);
        }

        /// <summary>
        /// Disposes the ArduinoDriver instance.
        /// </summary>
        public void Dispose()
        {
            try
            {
                port.Close();
                port.Dispose();
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        #region Private Methods

        private void Initialize(ArduinoDriverConfiguration config)
        {
            logger.Info("Instantiating ArduinoDriver: {0} - {1}...", config.ArduinoModel, config.PortName);

            this.config = config;

            switch (config.ArduinoModel)
            {
                case ArduinoModel.Micro:
                {
                    engineFunc = () => new RJCPSerialPortStream {PortName = config.PortName, BaudRate = DriverBaudRate};
                    break;
                }
                default:
                {
                    engineFunc = () => new DefaultSerialPort { PortName = config.PortName, BaudRate = DriverBaudRate };
                    break;
                }
            }

            if (!config.AutoBootstrap) InitializeWithoutAutoBootstrap();
            else InitializeWithAutoBootstrap();
        }

        private void InitializeWithoutAutoBootstrap()
        {
            // Without auto bootstrap, we just try to send a handshake request (the listener should already be
            // deployed). If that fails, we try nothing else.
            logger.Info("Initiating handshake...");
            InitializePort();
            var handshakeResponse = ExecuteHandshake();
            if (handshakeResponse == null)
            {
                port.Close();
                port.Dispose();
                throw new IOException(
                    string.Format(
                        "Unable to get a handshake ACK when sending a handshake request to the Arduino on port {0}. "
                        +
                        "Pass 'true' for optional parameter autoBootStrap in one of the ArduinoDriver constructors to "
                        + "automatically configure the Arduino (please note: this will overwrite the existing sketch "
                        + "on the Arduino).", config.PortName));
            }
        }

        private void InitializeWithAutoBootstrap()
        {
            var alwaysReDeployListener = alwaysRedeployListeners.Count > 500;
            HandShakeResponse handshakeResponse = null;
            var handShakeAckReceived = false;
            var handShakeIndicatesOutdatedProtocol = false;
            if (!alwaysReDeployListener)
            {
                logger.Info("Initiating handshake...");
                InitializePort();
                handshakeResponse = ExecuteHandshake();
                handShakeAckReceived = handshakeResponse != null;
                if (handShakeAckReceived)
                {
                    logger.Info("Handshake ACK Received ...");
                    const float currentVersion = (float)CurrentProtocolMajorVersion + (float)CurrentProtocolMinorVersion / 10;
                    var listenerVersion = handshakeResponse.ProtocolMajorVersion + (float)handshakeResponse.ProtocolMinorVersion / 10;
                    logger.Info("Current ArduinoDriver C# Protocol: {0}.", currentVersion);
                    logger.Info("Arduino Listener Protocol Version: {0}.", listenerVersion);
                    handShakeIndicatesOutdatedProtocol = currentVersion > listenerVersion;
                }
                else
                {
                    logger.Debug("Closing port...");
                    port.Close();
                    port.Dispose();
                }
            }

            // If we have received a handshake ack, and we have no need to upgrade, simply return.
            if (handShakeAckReceived && !handShakeIndicatesOutdatedProtocol) return;

            var arduinoModel = config.ArduinoModel;
            // At this point we will have to redeploy our listener
            logger.Info("Boostrapping ArduinoDriver Listener...");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            ExecuteAutoBootStrap(arduinoModel, config.PortName);
            stopwatch.Stop();
            logger.Info("Bootstrapping device took {0}ms.", stopwatch.ElapsedMilliseconds);

            // Now wait a bit, since the bootstrapped Arduino might still be restarting !
            var graceTime =
                rebootGraceTimes.ContainsKey(arduinoModel) ?
                    rebootGraceTimes[arduinoModel] : defaultRebootGraceTime;
            logger.Info("Grace time after reboot, waiting {0}ms...", graceTime);
            Thread.Sleep(graceTime);


            // Listener should now (always) be deployed, handshake should yield success.
            InitializePort();
            handshakeResponse = ExecuteHandshake();
            if (handshakeResponse == null)
                throw new IOException("Unable to get a handshake ACK after executing auto bootstrap on the Arduino!");  
            logger.Info("ArduinoDriver fully initialized!");      
        }

        private void InitializePort()
        {
            var engine = engineFunc();
            var portName = engine.PortName;
            var baudRate = engine.BaudRate;
            logger.Debug("Initializing port {0} - {1}...", portName, baudRate);
            engine.WriteTimeout = 100;
            engine.ReadTimeout = 100;
            port = new ArduinoDriverSerialPort(engine);
            port.Open();
        }

        private HandShakeResponse ExecuteHandshake()
        {
            var response = port.Send(new HandShakeRequest(), 1);
            return response as HandShakeResponse;            
        }

        private static void ExecuteAutoBootStrap(ArduinoModel arduinoModel, string portName)
        {
            logger.Info("Executing AutoBootStrap!");
            logger.Info("Deploying protocol version {0}.{1}.", CurrentProtocolMajorVersion, CurrentProtocolMinorVersion);
           
            logger.Debug("Reading internal resource stream with Arduino Listener HEX file...");
            var assembly = Assembly.GetExecutingAssembly();
            var textStream = assembly.GetManifestResourceStream(
                string.Format(ArduinoListenerHexResourceFileName, arduinoModel));
            if (textStream == null) 
                throw new IOException("Unable to configure auto bootstrap, embedded resource missing!");

            var hexFileContents = new List<string>();
            using (var reader = new StreamReader(textStream))
                while (reader.Peek() >= 0) hexFileContents.Add(reader.ReadLine());

            logger.Debug("Uploading HEX file...");
            var uploader = new ArduinoSketchUploader(new ArduinoSketchUploaderOptions
            {
                PortName = portName,
                ArduinoModel = arduinoModel
            });
            uploader.UploadSketch(hexFileContents);
        }

        #endregion
    }
}
