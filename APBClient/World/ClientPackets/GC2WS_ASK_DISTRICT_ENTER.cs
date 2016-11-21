using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APBClient.Networking;

namespace APBClient.World
{
    class GC2WS_ASK_DISTRICT_ENTER : ClientPacket
    {
        public GC2WS_ASK_DISTRICT_ENTER()
        {
            OpCode = (uint)APBOpCode.GC2WS_ASK_DISTRICT_ENTER;

            AllocateData(9);
            Writer.Write((byte)0);
        }
    }
}
