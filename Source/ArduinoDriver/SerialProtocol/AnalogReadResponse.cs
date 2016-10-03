namespace ArduinoDriver.SerialProtocol
{
    public class AnalogReadResponse : ArduinoResponse
    {
        public int PinRead { get; private set; }
        public int PinValue { get; private set; }

        public AnalogReadResponse(int pinRead, byte byte1, byte byte2)
        {
            PinRead = (byte) pinRead;
            PinValue = (byte1 << 8) + byte2;
        }
    }
}
