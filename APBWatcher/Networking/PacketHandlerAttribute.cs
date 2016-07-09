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
        public readonly LobbyOpCode OpCode;

        public PacketHandlerAttribute(LobbyOpCode opCode)
        {
            OpCode = opCode;
        }
    }
}
