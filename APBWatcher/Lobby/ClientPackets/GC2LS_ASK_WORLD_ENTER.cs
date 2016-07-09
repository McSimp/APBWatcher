using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APBWatcher.Networking;

namespace APBWatcher.Lobby.ClientPackets
{
    internal class GC2LS_ASK_WORLD_ENTER : ClientPacket
    {
        public GC2LS_ASK_WORLD_ENTER(int characterSlot)
        {
            OpCode = 1012;
            AllocateData(9);
            Writer.Write((byte)characterSlot);
        }
    }
}
