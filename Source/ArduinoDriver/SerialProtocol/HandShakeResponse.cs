namespace ArduinoDriver.SerialProtocol
{
    public class HandShakeResponse : ArduinoResponse
    {
        public int ProtocolMajorVersion { get; private set; }
        public int ProtocolMinorVersion { get; private set; }

        public HandShakeResponse(byte protocolMajorVersion, byte protocolMinorVersion)
        {
            ProtocolMajorVersion = protocolMajorVersion;
            ProtocolMinorVersion = protocolMinorVersion;
        }
    }
}
