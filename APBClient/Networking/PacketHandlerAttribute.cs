using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APBClient.Lobby;

namespace APBClient.Networking
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
