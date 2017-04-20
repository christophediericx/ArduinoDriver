using System;
using System.Threading.Tasks;

namespace ArduinoDriver.SerialEngines
{
    public interface ISerialPortEngine : IDisposable
    {
        string PortName { get; set; }
        int BaudRate { get; set; }
        int ReadTimeout { get; set; }
        int WriteTimeout { get; set; }
        void Open();
        void Close();
        Task WriteAsync(byte[] buffer, int offset, int count);
        Task<int> ReadAsync(byte[] buffer, int offset, int count);
    }
}
