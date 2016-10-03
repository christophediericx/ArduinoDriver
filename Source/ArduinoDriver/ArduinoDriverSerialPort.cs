using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using ArduinoDriver.SerialProtocol;
using NLog;

namespace ArduinoDriver
{
    internal class ArduinoDriverSerialPort : SerialPort
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly object serialDataIncoming;
        private const int ReceiveTimeOut = 1000;
        private const int MaxSendRetries = 5;
        private const int MaxSyncRetries = 10;
        private int numberOfBytesToRead;

        internal ArduinoDriverSerialPort(string portName, int baudRate)
            : base(portName, baudRate)
        {
            serialDataIncoming = new object();
            DataReceived += OnDataReceived;
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs serialDataReceivedEventArgs)
        {
            var availableBytes = BytesToRead;
            if (availableBytes < numberOfBytesToRead) return;
            lock (serialDataIncoming)
            {
                Monitor.Pulse(serialDataIncoming);
            }
        }

        private bool GetSync()
        {
            var bytes = CommandConstants.SyncBytes;
            numberOfBytesToRead = 4;
            Write(bytes, 0, bytes.Length);
            WaitForBytes(numberOfBytesToRead);
            var responseBytes = ReadCurrentReceiveBuffer(numberOfBytesToRead);
            return responseBytes.Length == 4
                   && responseBytes[0] == bytes[3]
                   && responseBytes[1] == bytes[2]
                   && responseBytes[2] == bytes[1]
                   && responseBytes[3] == bytes[0];
        }

        private bool ExecuteCommandHandShake(byte command, byte length)
        {
            var bytes = new byte[] {0xfb, command, length};
            numberOfBytesToRead = 3;
            Write(bytes, 0, bytes.Length);
            WaitForBytes(numberOfBytesToRead);
            var responseBytes = ReadCurrentReceiveBuffer(numberOfBytesToRead);
            return responseBytes.Length == 3
                   && responseBytes[0] == bytes[2]
                   && responseBytes[1] == bytes[1]
                   && responseBytes[2] == bytes[0];
        }

        private void WaitForBytes(int numberOfBytes)
        {
            numberOfBytesToRead = numberOfBytes;
            lock (serialDataIncoming)
            {
                if (!Monitor.Wait(serialDataIncoming, ReceiveTimeOut))
                    throw new TimeoutException();
            }            
        }

        internal ArduinoResponse Send(ArduinoRequest request)
        {
            var sendRetries = 0;
            while (sendRetries++ < MaxSendRetries - 1)
            {
                try
                {
                    // First try to get sync (send FF FE FD FC and require FC FD FE FF as a response).
                    bool hasSync;
                    var syncRetries = 0;
                    while (!(hasSync = GetSync()) && syncRetries++ < MaxSyncRetries - 1)
                    {
                        logger.Debug("Unable to get sync ... trying again ({0}/{1}).", syncRetries, MaxSyncRetries);
                    }
                    if (!hasSync)
                    {
                        var errorMessage = string.Format("Unable to get sync after {0} tries!", MaxSyncRetries);
                        logger.Fatal(errorMessage);
                        throw new IOException(errorMessage);
                    }

                    // Now send the command handshake (send FB as start of message marker + the command byte + length byte).
                    // Expect the inverse (length byte followed by command byte followed by FB) as a command ACK.
                    var requestBytes = request.Bytes.ToArray();
                    var requestBytesLength = requestBytes.Length;

                    if (!ExecuteCommandHandShake(request.Command, (byte)requestBytesLength))
                    {
                        var errorMessage = string.Format("Unable to configure command handshake for command {0}.", request);
                        logger.Fatal(errorMessage);
                        throw new IOException(errorMessage);
                    }

                    // Write out all packet bytes, followed by a Fletcher 16 checksum!
                    // Packet bytes consist of:
                    // 1. Command byte repeated
                    // 2. Request length repeated
                    // 3. The actual request bytes
                    // 4. Two fletcher-16 checksum bytes calculated over (1 + 2 + 3)
                    var packetBytes = new byte[requestBytesLength + 4];
                    packetBytes[0] = request.Command;
                    packetBytes[1] = (byte)requestBytesLength;
                    Buffer.BlockCopy(requestBytes, 0, packetBytes, 2, requestBytesLength);
                    var fletcher16CheckSum = CalculateFletcher16Checksum(packetBytes, requestBytesLength + 2);
                    var f0 = (byte)(fletcher16CheckSum & 0xff);
                    var f1 = (byte)((fletcher16CheckSum >> 8) & 0xff);
                    var c0 = (byte)(0xff - (f0 + f1) % 0xff);
                    var c1 = (byte)(0xff - (f0 + c0) % 0xff);
                    packetBytes[requestBytesLength + 2] = c0;
                    packetBytes[requestBytesLength + 3] = c1;

                    Write(packetBytes, 0, requestBytesLength + 4);

                    // Write out all bytes written marker (FA)
                    Write(new[] { CommandConstants.AllBytesWritten }, 0, 1);

                    // Expect response message to drop to be received in the following form:
                    // F9 (start of response marker) followed by response length
                    numberOfBytesToRead = 2;
                    WaitForBytes(numberOfBytesToRead);
                    var responseBytes = ReadCurrentReceiveBuffer(numberOfBytesToRead);
                    var startOfResponseMarker = responseBytes[0];
                    var responseLength = responseBytes[1];
                    if (startOfResponseMarker != CommandConstants.StartOfResponseMarker)
                    {
                        var errorMessage = string.Format("Did not receive start of response marker but {0}!", startOfResponseMarker);
                        logger.Fatal(errorMessage);
                        throw new IOException(errorMessage);
                    }

                    // Read x responsebytes
                    numberOfBytesToRead = responseLength;
                    if (BytesToRead < numberOfBytesToRead) WaitForBytes(numberOfBytesToRead);
                    responseBytes = ReadCurrentReceiveBuffer(numberOfBytesToRead);
                    return ArduinoResponse.Create(responseBytes);
                }
                catch (TimeoutException ex)
                {
                    logger.Debug(ex, "TimeoutException in Send occurred, retrying ({0}/{1})!", sendRetries, MaxSendRetries);
                }
                catch (Exception ex)
                {
                    logger.Debug(ex, "General exception in Send occurred, retrying ({0}/{1})!", sendRetries, MaxSendRetries);
                }                
            }
            return null;
        }

        private static ushort CalculateFletcher16Checksum(IReadOnlyList<byte> checkSumBytes, int count)
        {
            ushort sum1 = 0;
            ushort sum2 = 0;
            for (var index = 0; index < count; ++index)
            {
                sum1 = (ushort)((sum1 + checkSumBytes[index])%255);
                sum2 = (ushort)((sum2 + sum1) % 255);
            }
            return (ushort)((sum2 << 8) | sum1);
        }

        private byte[] ReadCurrentReceiveBuffer(int numberOfBytes)
        {
            var result = new byte[numberOfBytes];
            Read(result, 0, numberOfBytes);
            return result;
        }
    }
}
