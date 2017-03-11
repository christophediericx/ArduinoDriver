namespace ArduinoDriver.SerialProtocol
{
    public class AnalogReferenceRequest : ArduinoRequest
    {
        public AnalogReferenceRequest(AnalogReferenceType analogReferenceType) 
            : base(CommandConstants.AnalogReference)
        {
            Bytes.Add((byte) analogReferenceType);
        }
    }
}
