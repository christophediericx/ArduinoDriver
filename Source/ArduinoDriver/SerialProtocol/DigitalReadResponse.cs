namespace ArduinoDriver.SerialProtocol
{
    public class DigitalReadResponse : ArduinoResponse
    {
        public int PinRead { get; private set; }
        public DigitalValue PinValue { get; private set; }

        public DigitalReadResponse(byte pinRead, DigitalValue value)
        {
            PinRead = pinRead;
            PinValue = value;
        }
    }
}
