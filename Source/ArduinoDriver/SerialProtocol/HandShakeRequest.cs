namespace ArduinoDriver.SerialProtocol
{
    public class HandShakeRequest : ArduinoRequest
    {
        public HandShakeRequest()
            : base(CommandConstants.HandshakeInitiate)
        {
        }
    }
}
