using System;

namespace ArduinoLibCSharp.IntelHexFormatReader
{
    public class MemoryCell
    {
        public long Address { get; private set; }
        public bool Modified { get; set; }
        public byte Value { get; set; }

        public MemoryCell(long address)
        {
            Address = address;
        }

        public override string ToString()
        {
            return string.Format("MemoryCell :{0} Value: {1} (modified = {2})",
                Address.ToString("X"), Value, Modified);
        }
    }
}
