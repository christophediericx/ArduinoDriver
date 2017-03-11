namespace ArduinoDriver.SerialProtocol
{
    public class AnalogReferenceResponse : ArduinoResponse
    {
        public AnalogReferenceType ReferenceType { get; private set; }

        public AnalogReferenceResponse(AnalogReferenceType referenceType)
        {
            ReferenceType = referenceType;
        }
    }
}
