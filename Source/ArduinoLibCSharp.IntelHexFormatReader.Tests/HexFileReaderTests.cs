using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArduinoLibCSharp.IntelHexFormatReader.Tests
{
    [TestClass]
    public class HexFileReaderTests
    {
        // Straight from Wikipedia
        private readonly string[] validSnippet = new[]
        {
            ":10010000214601360121470136007EFE09D2190140",
            ":100110002146017E17C20001FF5F16002148011928",
            ":10012000194E79234623965778239EDA3F01B2CAA7",
            ":100130003F0156702B5E712B722B732146013421C7",
            ":00000001FF"
        };

        private readonly string[] wrongCheckSum = new[]
        {
            ":10010000214601360121470136007EFE09D21901B3",
            ":00000001FF"
        };

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HexFileReaderThrowsExceptionWhenHexFileContentsIsNull()
        {
            var reader = new HexFileReader(null, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HexFileReaderThrowsExceptionWhenMemorySizeIsInvalid()
        {
            var reader = new HexFileReader(new List<string>(), 0);
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void HexFileReaderThrowsExceptionWhenTryingToWriteOutOfBounds()
        {
            var reader = new HexFileReader(validSnippet, 32);
            var memory = reader.Parse();
        }

        [TestMethod]
        public void HexFileReaderAcceptsValidSnippetAndReturnsCorrectMemoryModel()
        {
            var reader = new HexFileReader(validSnippet, 2048);
            var memory = reader.Parse();
            for (var i = 0; i < 64; i++)
                memory.Cells[i + 256].Modified.Should().BeTrue();

            // Check contents of first line.
            memory.Cells[256].Value.Should().Be(33);
            memory.Cells[257].Value.Should().Be(70);
            memory.Cells[258].Value.Should().Be(1);
            memory.Cells[259].Value.Should().Be(54);
            memory.Cells[260].Value.Should().Be(1);
            memory.Cells[261].Value.Should().Be(33);
            memory.Cells[262].Value.Should().Be(71);
            memory.Cells[263].Value.Should().Be(1);
            memory.Cells[264].Value.Should().Be(54);
            memory.Cells[265].Value.Should().Be(0);
            memory.Cells[266].Value.Should().Be(126);
            memory.Cells[267].Value.Should().Be(254);
            memory.Cells[268].Value.Should().Be(9);
            memory.Cells[269].Value.Should().Be(210);
            memory.Cells[270].Value.Should().Be(25);
            memory.Cells[271].Value.Should().Be(1);
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void HexFileReaderThrowsExceptionForInvalidCheckSum()
        {
            var reader = new HexFileReader(wrongCheckSum, 2048);
            var memory = reader.Parse();
        }
    }
}
