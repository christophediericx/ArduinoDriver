namespace ArduinoLibCSharp.IntelHexFormatReader
{
    internal class IntelHexRecord
    {
        public int ByteCount { get; set; }
        public int Address { get; set; }
        public int RecordType { get; set; }
        public byte[] Bytes { get; set; }
        public int CheckSum { get; set; }
    }
}
