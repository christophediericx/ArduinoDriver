namespace ArduinoLibCSharp.ArduinoDriver.SerialProtocol
{
    public class AnalogWriteRequest : ArduinoRequest
    {
        public AnalogWriteRequest(byte pinToWrite, byte valueToWrite)
            : base(CommandConstants.AnalogWrite)
        {
            Bytes.Add(pinToWrite);
            Bytes.Add(valueToWrite);
        }
    }
}
