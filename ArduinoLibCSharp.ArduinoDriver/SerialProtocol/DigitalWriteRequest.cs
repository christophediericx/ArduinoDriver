namespace ArduinoLibCSharp.ArduinoDriver.SerialProtocol
{
    public class DigitalWriteRequest : ArduinoRequest
    {
        public DigitalWriteRequest(byte pinToWrite, DigitalValue pinValue)
            : base(CommandConstants.DigitalWrite)
        {
            Bytes.Add(pinToWrite);
            Bytes.Add((byte)pinValue);
        }
    }
}
