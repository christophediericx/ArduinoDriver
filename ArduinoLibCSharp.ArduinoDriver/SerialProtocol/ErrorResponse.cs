namespace ArduinoLibCSharp.ArduinoDriver.SerialProtocol
{
    public class ErrorResponse : ArduinoResponse
    {
        public byte Byte1 { get; private set; }
        public byte Byte2 { get; private set; }
        public byte Byte3 { get; private set; }

        public ErrorResponse(byte byte1, byte byte2, byte byte3)
        {
            Byte1 = byte1;
            Byte2 = byte2;
            Byte3 = byte3;
        }
    }
}
