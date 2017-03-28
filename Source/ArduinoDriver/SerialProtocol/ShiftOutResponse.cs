namespace ArduinoDriver.SerialProtocol
{
    public class ShiftOutResponse : ArduinoResponse
    {
        public int DigitalPin { get; private set; }
        public int ClockPin { get; private set; }
        public BitOrder BitOrder { get; private set; }
        public byte Value { get; private set; }

        public ShiftOutResponse(byte digitalPin, byte clockPin, BitOrder bitOrder, byte value)
        {
            DigitalPin = digitalPin;
            ClockPin = clockPin;
            BitOrder = bitOrder;
            Value = value;
        }
    }
}
