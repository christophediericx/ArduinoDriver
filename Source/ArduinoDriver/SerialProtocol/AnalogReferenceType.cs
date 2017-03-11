namespace ArduinoDriver.SerialProtocol
{
    public enum AnalogReferenceType
    {
        Default,
        Internal,       // Not for Arduino MEGA
        Internal1v1,    // Arduino MEGA only
        Internal2v56,   // Arduino MEGA only
        External
    }
}
