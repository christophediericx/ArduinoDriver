using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Threading;
using ArduinoLibCSharp.ArduinoDriver.SerialProtocol;
using ArduinoUploader;
using NLog;

namespace ArduinoLibCSharp.ArduinoDriver
{
    /// <summary>
    /// An ArduinoDriver can be used to communicate with an attached Arduino by way of sending requests and receiving responses.
    /// These messages are sent over a live serial connection (via a serial protocol) to the Arduino. The required listener can be 
    /// automatically deployed to the Arduino.
    /// </summary>
    public class ArduinoDriver
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private const int CurrentProtocolMajorVersion = 1;
        private const int CurrentProtocolMinorVersion = 0;
        private const int GraceTimeAfterArduinoAutoBootStrap = 5000;
        private ArduinoDriverSerialPort port;

        private const string ArduinoListenerHexResourceFileName =
            "ArduinoLibCSharp.ArduinoDriver.ArduinoListener.ArduinoListener.ino.hex";

        /// <summary>
        /// Creates a new ArduinoDriver instance. The relevant portname will be autodetected if possible.
        /// </summary>
        /// <param name="autoBootstrap"></param>
        public ArduinoDriver(bool autoBootstrap = false)
        {
            logger.Info("Instantiating ArduinoDriver with autoconfiguration of port name...");
            var possiblePortNames = SerialPort.GetPortNames();
            string unambiguous = null;
            try
            {
                unambiguous = possiblePortNames.SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                // More than one posible hit.
            }
            if (unambiguous == null)
                throw new IOException("Unable to autoconfigure ArduinoDriver port name, since there is not exactly a single COM port available. Please use the "
                    + "ArduinoDriver with the named port constructor!");
            Initialize(unambiguous, autoBootstrap);
        }

        /// <summary>
        /// Creates a new ArduinoDriver instance for a specified portName.
        /// </summary>
        /// <param name="portName">The COM portname to create the ArduinoDriver instance for.</param>
        /// <param name="autoBootStrap">Determines if an listener is automatically deployed to the Arduino if required.</param>
        public ArduinoDriver(string portName, bool autoBootStrap = false)
        {
            Initialize(portName, autoBootStrap);
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
        /// Closes the ArduinoDriver instance.
        /// </summary>
        public void Close()
        {
            try
            {
                port.Close();
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        #region Private Methods

        private void Initialize(string portName, bool autoBootStrap)
        {
            logger.Info("Instantiating ArduinoDriver for port {0}...", portName);
            port = new ArduinoDriverSerialPort(portName, 115200);
            port.Open();

            logger.Info("Initiating handshake...");
            var response = port.Send(new HandShakeRequest());
            var handshakeResponse = response as HandShakeResponse;
            if (handshakeResponse == null)
            {
                logger.Info("Received null for handshake (timeout?).");
                if (!autoBootStrap)
                    throw new IOException(
                        string.Format(
                            "Unable to get a handshake ACK when sending a handshake request to the Arduino on port {0}. " +
                            "Pass 'true' for optional parameter autoBootStrap in one of the ArduinoDriver constructors to automatically configure the Arduino (please " +
                            "note: this will overwrite the existing sketch on the Arduino).", portName));
                ExecuteAutoBootStrap();
            }
            else
            {
                logger.Info("Handshake ACK Received ... checking if we need to upgrade!");
                const float currentVersion = (float)CurrentProtocolMajorVersion + (float)CurrentProtocolMinorVersion / 10;
                var listenerVersion = handshakeResponse.ProtocolMajorVersion + (float)handshakeResponse.ProtocolMinorVersion / 10;
                logger.Info("Current ArduinoDriver C# Protocol: {0}.", currentVersion);
                logger.Info("Arduino Listener Protocol Version: {0}.", listenerVersion);
                var upgradeNeeded = currentVersion > listenerVersion;
                logger.Info("Upgrade neede: {0}", upgradeNeeded);
                if (upgradeNeeded) ExecuteAutoBootStrap();
            }            
        }

        private void ExecuteAutoBootStrap()
        {
            logger.Info("Executing AutoBootStrap!");
            logger.Info("Deploying protocol version {0}.{1}.", CurrentProtocolMajorVersion, CurrentProtocolMinorVersion);
            var portName = port.PortName;
            logger.Debug("Closing port {0}...", portName);
            port.Close();

            logger.Debug("Reading internal resource stream with Arduino Listener HEX file...");
            var assembly = Assembly.GetExecutingAssembly();
            var textStream = assembly.GetManifestResourceStream(ArduinoListenerHexResourceFileName);
            if (textStream == null) throw new IOException("Unable to configure auto bootstrap, embedded resource missing!");
            var hexFileContents = new List<string>();
            using (var reader = new StreamReader(textStream))
                while (reader.Peek() >= 0) hexFileContents.Add(reader.ReadLine());

            logger.Debug("Uploading HEX file...");
            var uploader = new ArduinoSketchUploader(new ArduinoSketchUploaderOptions
            {
                PortName = portName
            });
            uploader.UploadSketch(hexFileContents);

            logger.Debug("Reopening port {0}...", portName);
            port.Open();

            // Now wait a bit, the Arduino might still be restarting!
            Thread.Sleep(GraceTimeAfterArduinoAutoBootStrap);

            // After bootstrapping, check if we can succesfully handshake ...
            var response = port.Send(new HandShakeRequest());
            var handshakeResponse = response as HandShakeResponse;
            if (handshakeResponse == null) throw new IOException("Unable to get a handshake ACK after executing auto bootstrap on the Arduino!");
            logger.Info("Arduino (auto)bootstrapped succesfully!");
        }

        #endregion
    }
}
