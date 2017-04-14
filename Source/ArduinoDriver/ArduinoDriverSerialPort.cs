using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArduinoDriver.SerialEngines;
using ArduinoDriver.SerialProtocol;
using NLog;

namespace ArduinoDriver
{
    internal class ArduinoDriverSerialPort
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ISerialPortEngine serialPort;
        private const int MaxSendRetries = 6;
        private const int MaxSyncRetries = 10;

        internal ArduinoDriverSerialPort(ISerialPortEngine serialPortEngine)
        {
            serialPort = serialPortEngine;
        }

        public void Open()
        {
            serialPort.Open();
        }

        public void Close()
        {
            serialPort.Close();
        }

        public void Dispose()
        {
            serialPort.Dispose();
        }

        private async Task<bool> SynchronizeAsync()
        {
            try
            {
                var bytes = CommandConstants.SyncBytes;
                await serialPort.WriteAsync(bytes, 0, bytes.Length);
                var responseBytes = await ReadCurrentReceiveBufferAsync(4);
                return responseBytes.Length == 4
                       && responseBytes[0] == bytes[3]
                       && responseBytes[1] == bytes[2]
                       && responseBytes[2] == bytes[1]
                       && responseBytes[3] == bytes[0];
            }
            catch (TimeoutException e)
            {
                logger.Trace("Timeout while trying to get sync...");
                return false;
            }
            catch (Exception e)
            {
                logger.Trace("Exception while trying to get sync ('{0}')", e.Message);
                return false;
            }
        }

        private async Task<bool> ExecuteCommandHandShakeAsync(byte command, byte length)
        {
            var bytes = new byte[] {0xfb, command, length};
            await serialPort.WriteAsync(bytes, 0, bytes.Length);
            var responseBytes = await ReadCurrentReceiveBufferAsync(3);
            return responseBytes.Length == 3
                   && responseBytes[0] == bytes[2]
                   && responseBytes[1] == bytes[1]
                   && responseBytes[2] == bytes[0];
        }

        internal async Task<ArduinoResponse> SendAsync(ArduinoRequest request, int maxSendRetries = MaxSendRetries)
        {
            logger.Trace("Sending {0}", request);
            var sendRetries = 0;
            while (sendRetries++ < maxSendRetries)
            {
                try
                {
                    // First try to get sync (send FF FE FD FC and require FC FD FE FF as a response).
                    bool hasSync;
                    var syncRetries = 0;
                    while (!(hasSync = await SynchronizeAsync()) && syncRetries++ < MaxSyncRetries)
                    {
                        logger.Debug("Unable to get sync ... trying again ({0}/{1}).", syncRetries, MaxSyncRetries);
                    }
                    if (!hasSync)
                    {
                        var errorMessage = string.Format("Unable to get sync after {0} tries!", MaxSyncRetries);
                        logger.Warn(errorMessage);
                        throw new IOException(errorMessage);
                    }

                    // Now send the command handshake (send FB as start of message marker + the command byte + length byte).
                    // Expect the inverse (length byte followed by command byte followed by FB) as a command ACK.
                    var requestBytes = request.Bytes.ToArray();
                    var requestBytesLength = requestBytes.Length;

                    if (!await ExecuteCommandHandShakeAsync(request.Command, (byte)requestBytesLength))
                    {
                        var errorMessage = string.Format("Unable to configure command handshake for command {0}.", request);
                        logger.Warn(errorMessage);
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

                    await serialPort.WriteAsync(packetBytes, 0, requestBytesLength + 4);

                    // Write out all bytes written marker (FA)
                    await serialPort.WriteAsync(new[] { CommandConstants.AllBytesWritten }, 0, 1);

                    // Expect response message to drop to be received in the following form:
                    // F9 (start of response marker) followed by response length
                    var responseBytes = await ReadCurrentReceiveBufferAsync(2);
                    var startOfResponseMarker = responseBytes[0];
                    var responseLength = responseBytes[1];
                    if (startOfResponseMarker != CommandConstants.StartOfResponseMarker)
                    {
                        var errorMessage = string.Format("Did not receive start of response marker but {0}!", startOfResponseMarker);
                        logger.Warn(errorMessage);
                        throw new IOException(errorMessage);
                    }

                    // Read x responsebytes
                    responseBytes = await ReadCurrentReceiveBufferAsync(responseLength);
                    return ArduinoResponse.Create(responseBytes);
                }
                catch (TimeoutException ex)
                {
                    logger.Debug(ex, "TimeoutException in Send occurred, retrying ({0}/{1})!", sendRetries, MaxSendRetries);
                }
                catch (Exception ex)
                {
                    logger.Debug("Exception: '{0}'", ex.Message);
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

        private async Task<byte[]> ReadCurrentReceiveBufferAsync(int numberOfBytes)
        {
            var result = new byte[numberOfBytes];
            var retrieved = 0;
            var retryCount = 0;

            while (retrieved < numberOfBytes && retryCount++ < 4)
                retrieved += await serialPort.ReadAsync(result, retrieved, numberOfBytes - retrieved);

            if (retrieved < numberOfBytes)
            {
                logger.Info("Ended up reading short (expected {0} bytes, got only {1})...",
                    numberOfBytes, retrieved);
            }
            return result;
        }
    }
}