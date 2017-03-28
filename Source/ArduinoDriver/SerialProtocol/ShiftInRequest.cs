namespace ArduinoDriver.SerialProtocol
{
    public class ShiftInRequest : ArduinoRequest
    {
        public ShiftInRequest(byte dataPin, byte clockPin, BitOrder bitOrder) 
            : base(CommandConstants.ShiftIn)
        {
            Bytes.Add(dataPin);
            Bytes.Add(clockPin);
            Bytes.Add((byte)bitOrder);
        }
    }
}
