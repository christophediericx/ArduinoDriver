namespace ArduinoLibCSharp.ArduinoDriver.SerialProtocol
{
    public class NoToneRequest : ArduinoRequest
    {
        public NoToneRequest(byte pinToWrite)
            : base(CommandConstants.NoTone)
        {
            Bytes.Add(pinToWrite);
        }
    }
}
