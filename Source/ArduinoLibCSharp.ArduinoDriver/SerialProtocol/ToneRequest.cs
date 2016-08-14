namespace ArduinoLibCSharp.ArduinoDriver.SerialProtocol
{
    public class ToneRequest : ArduinoRequest
    {
        public ToneRequest(byte pinToWrite, ushort frequency)
            : base(CommandConstants.Tone)
        {
            Write(pinToWrite, frequency, 0);
        }

        public ToneRequest(byte pinToWrite, ushort frequency, uint duration)
            : base(CommandConstants.Tone)
        {
            Write(pinToWrite, frequency, duration);
        }

        private void Write(byte pinToWrite, ushort frequency, uint duration)
        {
            Bytes.Add(pinToWrite);
            Bytes.Add((byte)(frequency >> 8));
            Bytes.Add((byte)(frequency & 0xff));
            Bytes.Add((byte)(duration >> 24));
            Bytes.Add((byte)(duration >> 16));
            Bytes.Add((byte)(duration >> 8));
            Bytes.Add((byte)(duration & 0xff));            
        }
    }
}
