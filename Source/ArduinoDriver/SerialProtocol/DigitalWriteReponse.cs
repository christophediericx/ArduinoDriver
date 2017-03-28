namespace ArduinoDriver.SerialProtocol
{
    public class DigitalWriteReponse : ArduinoResponse
    {
        public int PinWritten { get; private set; }
        public DigitalValue PinValue { get; private set; }

        public DigitalWriteReponse(byte pinRead, DigitalValue value)
        {
            PinWritten = pinRead;
            PinValue = value;
        }
    }
}
