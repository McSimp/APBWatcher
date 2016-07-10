using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using APBWatcher.Lobby;
using APBWatcher.Networking;

namespace APBWatcher.World
{
    public partial class WorldClient
    {
        [PacketHandler(APBOpCode.WS2GC_ANS_WORLD_ENTER)]
        private class WS2GC_ANS_WORLD_ENTER : BasePacketHandler<WorldClient>
        {
            public override void HandlePacket(WorldClient client, ServerPacket packet)
            {
                var reader = packet.Reader;

                Console.WriteLine(Util.HexDump(packet.Data));
            }
        }
    }
}
