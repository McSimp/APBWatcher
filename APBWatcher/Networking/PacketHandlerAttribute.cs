using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APBWatcher.Lobby;

namespace APBWatcher.Networking
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class PacketHandlerAttribute : Attribute
    {
        public readonly APBOpCode OpCode;

        public PacketHandlerAttribute(APBOpCode opCode)
        {
            OpCode = opCode;
        }
    }
}
