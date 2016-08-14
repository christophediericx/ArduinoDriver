using System.Collections.Generic;

namespace ArduinoLibCSharp.ArduinoDriver.SerialProtocol
{
    public abstract class ArduinoRequest : ArduinoMessage
    {
        protected ArduinoRequest(byte command)
        {
            Command = command;
        }

        internal IList<byte> Bytes = new List<byte>();
        internal byte Command { get; private set; }
    }
}
