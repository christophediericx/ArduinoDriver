namespace ArduinoDriver.SerialProtocol
{
    public class ShiftOutRequest : ArduinoRequest
    {
        public ShiftOutRequest(byte dataPin, byte clockPin, BitOrder bitOrder, byte value) 
            : base(CommandConstants.ShiftOut)
        {
            Bytes.Add(dataPin);
            Bytes.Add(clockPin);
            Bytes.Add((byte)bitOrder);
            Bytes.Add(value);
        }
    }
}
