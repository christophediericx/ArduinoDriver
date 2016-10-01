using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArduinoLibCSharp.IntelHexFormatReader
{
    /// <summary>
    /// The HexFileReader can read the contents of an Intel HEX file, and returns a "MemoryBlock" model when the Parse method is called.
    /// </summary>
    public class HexFileReader
    {
        private readonly IEnumerable<string> hexFileContents;
        private readonly int memorySize;

        public HexFileReader(string fileName, int memorySize) 
            : this (File.ReadLines(fileName), memorySize)
        {
        }

        public HexFileReader(IEnumerable<string> hexFileContents, int memorySize)
        {
            if (hexFileContents == null) throw new ArgumentException("Hex file contents can not be null!");
            if (memorySize <= 0) throw new ArgumentException("Memory size must be greater than zero!");
            this.hexFileContents = hexFileContents;
            this.memorySize = memorySize;
        }

        public MemoryBlock Parse()
        {
            return ReadHexFile(hexFileContents, memorySize);
        }

        private static MemoryBlock ReadHexFile(IEnumerable<string> fileContents, int memorySize)
        {
            var result = new MemoryBlock { Cells = new MemoryCell[memorySize] };
            for (var i = 0; i < memorySize; i++)
                result.Cells[i] = new MemoryCell() {Value = 0xff};

            var lineNumber = 0;
            var baseAddress = 0;
            var maxAddress = 0;
            foreach (var line in fileContents)
            {
                var buffer = line;
                lineNumber++;
                if (buffer[0] != ':') continue;
                var ihex = ParseLine(buffer);
                switch (ihex.RecordType)
                {
                    case 0: 
                    {
                        var nextAddress = ihex.Address + baseAddress;
                        for (var i = 0; i < ihex.ByteCount; i++)
                        {
                            if (nextAddress + i > memorySize)
                                throw new IOException(
                                    string.Format("Trying to write to position {0} outside of memory boundaries ({1})!", nextAddress + i, memorySize));
                            var cell = result.Cells[nextAddress + i];
                            cell.Value = ihex.Bytes[i];
                            cell.Modified = true;
                        }
                        if (nextAddress + ihex.ByteCount > maxAddress) maxAddress = nextAddress + ihex.ByteCount;
                        break;
                    }
                    case 2:
                    {
                        baseAddress = (ihex.Bytes[0] << 8 | ihex.Bytes[1]) << 4;
                        break;
                    }
                    case 4:
                    {
                        baseAddress = (ihex.Bytes[0] << 8 | ihex.Bytes[1]) << 16;
                        break;
                    }
                    case 1:
                    case 3:
                    case 5:
                        break;
                    default:
                        throw new IOException(string.Format("Unable to parse line {0} in Intel Hex file!", lineNumber));
                }
            }
            result.MaxAddress = maxAddress;
            return result;
        }

        private static IntelHexRecord ParseLine(string rec)
        {
            if (rec[0] != ':')
                throw new Exception();

            var hexByteCount = new string(rec.Skip(1).Take(2).ToArray());
            var byteCount = Convert.ToInt32(hexByteCount, 16);

            var hexAddress = new string(rec.Skip(3).Take(4).ToArray());
            var address = Convert.ToInt32(hexAddress, 16);

            var hexRecType = new string(rec.Skip(7).Take(2).ToArray());
            var recType = Convert.ToInt32(hexRecType, 16);

            var hexData = new string(rec.Skip(9).Take(2 * byteCount).ToArray());
            var bytes = new byte[byteCount];
            var counter = 0;
            foreach (var hexByte in Split(hexData, 2))
                bytes[counter++] = (byte)Convert.ToInt32(hexByte, 16);

            var hexCheckSum = new string(rec.Skip(9 + 2 * byteCount).Take(2).ToArray());
            var checkSum = Convert.ToInt32(hexCheckSum, 16);

            // Calculate if checksum is correct.
            var allbytes = new byte[1 + 2 + 1 + byteCount];
            counter = 0;
            foreach (var hexByte in Split(new string(rec.Skip(1).Take((4 + byteCount) * 2).ToArray()), 2))
                allbytes[counter++] = (byte)Convert.ToInt32(hexByte, 16);

            var maskedSumBytes = allbytes.Sum(x => (ushort)x) & 0xff;
            var checkSumCalculated = (byte)(256 - maskedSumBytes);

            if (checkSumCalculated != checkSum)
                throw new IOException(string.Format("Checksum for line {0} is incorrect!", rec));

            return new IntelHexRecord
            {
                ByteCount = byteCount,
                Address = address,
                RecordType = recType,
                Bytes = bytes,
                CheckSum = checkSum
            };
        }

        private static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }
    }
}
