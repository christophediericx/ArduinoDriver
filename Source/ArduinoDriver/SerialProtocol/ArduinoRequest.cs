using System;
using System.Collections.Generic;
using System.Linq;

namespace ArduinoDriver.SerialProtocol
{
    public abstract class ArduinoRequest : ArduinoMessage
    {
        protected ArduinoRequest(byte command)
        {
            Command = command;
        }

        internal IList<byte> Bytes = new List<byte>();
        internal byte Command { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", GetType().FullName, 
                BitConverter.ToString(Bytes.ToArray()));
        }
    }
}
