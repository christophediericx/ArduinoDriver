namespace ArduinoDriver.SerialProtocol
{
    public class AnalogWriteResponse : ArduinoResponse
    {
        public int PinWritten { get; private set; }
        public int ValueWritten { get; private set; }

        public AnalogWriteResponse(byte pinRead, byte value)
        {
            PinWritten = pinRead;
            ValueWritten = value;
        }
    }
}
