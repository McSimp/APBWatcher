using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APBWatcher.Networking;

namespace APBWatcher.Lobby
{
    public partial class LobbyClient
    {
        private class GC2LS_ASK_WORLD_LIST : ClientPacket
        {
            public GC2LS_ASK_WORLD_LIST()
            {
                OpCode = (uint)LobbyOpCode.GC2LS_ASK_WORLD_LIST;
                AllocateData(8);
            }
        }
    }
}
