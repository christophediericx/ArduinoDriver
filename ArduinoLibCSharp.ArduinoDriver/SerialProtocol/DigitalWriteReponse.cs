namespace ArduinoLibCSharp.ArduinoDriver.SerialProtocol
{
    public class DigitalWriteReponse : ArduinoResponse
    {
        public int PinWritten { get; private set; }
        public DigitalValue PinValue { get; private set; }

        public DigitalWriteReponse(int pinRead, int value)
        {
            PinWritten = (byte)pinRead;
            PinValue = (byte)value == 1 ? DigitalValue.High : DigitalValue.Low;
        }
    }
}
