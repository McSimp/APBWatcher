using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APBWatcher.Lobby
{
    class GC2LS_ASK_WORLD_LIST : ClientPacket
    {
        public GC2LS_ASK_WORLD_LIST()
        {
            OpCode = 1007;
            AllocateData(4);
        }
    }
}
