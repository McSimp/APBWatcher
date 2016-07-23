using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APBClient.Networking;

namespace APBClient.Lobby
{
    public partial class LobbyClient
    {
        private class GC2LS_ASK_WORLD_LIST : ClientPacket
        {
            public GC2LS_ASK_WORLD_LIST()
            {
                OpCode = (uint)APBOpCode.GC2LS_ASK_WORLD_LIST;
                AllocateData(8);
            }
        }
    }
}
