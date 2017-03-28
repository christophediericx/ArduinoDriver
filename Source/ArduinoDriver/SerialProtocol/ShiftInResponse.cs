namespace ArduinoDriver.SerialProtocol
{
    public class ShiftInResponse : ArduinoResponse
    {
        public int DigitalPin { get; private set; }
        public int ClockPin { get; private set; }
        public BitOrder BitOrder { get; private set; }
        public byte Incoming { get; private set; }

        public ShiftInResponse(byte digitalPin, byte clockPin, BitOrder bitOrder, byte incoming)
        {
            DigitalPin = digitalPin;
            ClockPin = clockPin;
            BitOrder = bitOrder;
            Incoming = incoming;
        }
    }
}
