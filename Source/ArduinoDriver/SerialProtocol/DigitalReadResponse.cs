namespace ArduinoLibCSharp.ArduinoDriver.SerialProtocol
{
    public class DigitalReadResponse : ArduinoResponse
    {
        public int PinRead { get; private set; }
        public DigitalValue PinValue { get; private set; }

        public DigitalReadResponse(int pinRead, int value)
        {
            PinRead = (byte) pinRead;
            PinValue = (byte) value == 1 ? DigitalValue.High :  DigitalValue.Low;
        }
    }
}
