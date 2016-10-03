namespace ArduinoDriver.SerialProtocol
{
    public class HandShakeResponse : ArduinoResponse
    {
        public int ProtocolMajorVersion { get; private set; }
        public int ProtocolMinorVersion { get; private set; }

        public HandShakeResponse(int protocolMajorVersion, int protocolMinorVersion)
        {
            ProtocolMajorVersion = protocolMajorVersion;
            ProtocolMinorVersion = protocolMinorVersion;
        }
    }
}
