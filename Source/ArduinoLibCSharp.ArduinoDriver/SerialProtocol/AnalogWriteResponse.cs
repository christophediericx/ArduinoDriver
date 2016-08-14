namespace ArduinoLibCSharp.ArduinoDriver.SerialProtocol
{
    public class AnalogWriteResponse : ArduinoResponse
    {
        public int PinWritten { get; private set; }
        public int ValueWritten { get; private set; }

        public AnalogWriteResponse(int pinRead, int value)
        {
            PinWritten = (byte)pinRead;
            ValueWritten = value;
        }
    }
}
