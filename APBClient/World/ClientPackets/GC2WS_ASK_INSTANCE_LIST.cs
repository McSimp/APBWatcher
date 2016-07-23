using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APBClient.Networking;

namespace APBClient.World
{
    public partial class WorldClient
    {
        private class GC2WS_ASK_INSTANCE_LIST : ClientPacket
        {
            public GC2WS_ASK_INSTANCE_LIST()
            {
                OpCode = (uint)APBOpCode.GC2WS_ASK_INSTANCE_LIST;

                AllocateData(8);
            }
        }
    }
}
