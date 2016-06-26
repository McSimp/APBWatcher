using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APBWatcher.Lobby
{
    class GC2LS_ASK_WORLD_ENTER : ClientPacket
    {
        public GC2LS_ASK_WORLD_ENTER(int characterSlot)
        {
            OpCode = 1012;
            AllocateData(5);
            Writer.Write((byte)characterSlot);
        }
    }
}
