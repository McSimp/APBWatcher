using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APBWatcher.Networking;

namespace APBWatcher.World
{
    public partial class WorldClient
    {
        private class GC2WS_ASK_WORLD_ENTER : ClientPacket
        {
            public GC2WS_ASK_WORLD_ENTER(uint accountId, byte[] hash)
            {
                OpCode = (uint)APBOpCode.GC2WS_ASK_WORLD_ENTER;

                AllocateData(32);
                Writer.Write(accountId);
                Writer.Write(hash);
            }
        }
    }
}
