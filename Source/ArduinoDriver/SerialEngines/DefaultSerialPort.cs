using System.IO.Ports;
using System.Threading.Tasks;

namespace ArduinoDriver.SerialEngines
{
    public class DefaultSerialPort : SerialPort, ISerialPortEngine
    {
        Task ISerialPortEngine.WriteAsync(byte[] buffer, int offset, int count)
        {
            return BaseStream.WriteAsync(buffer, offset, count);
        }

        Task<int> ISerialPortEngine.ReadAsync(byte[] buffer, int offset, int count)
        {
            return BaseStream.ReadAsync(buffer, offset, count);
        }
    }
}
