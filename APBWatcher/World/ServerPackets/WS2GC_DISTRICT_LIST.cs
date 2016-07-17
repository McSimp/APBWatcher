using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using APBWatcher.Networking;

namespace APBWatcher.World
{
    public partial class WorldClient
    {
        [PacketHandler(APBOpCode.WS2GC_DISTRICT_LIST)]
        private class WS2GC_DISTRICT_LIST : BasePacketHandler<WorldClient>
        {
            public override void HandlePacket(WorldClient client, ServerPacket packet)
            {
                var reader = packet.Reader;

                int returnCode = reader.ReadInt32();
                if (returnCode != 0)
                {
                    Log.Error($"WS2GC_DISTRICT_LIST response had invalid return code {returnCode}");
                    client.OnWorldEnterFailed(client, returnCode);
                    return;
                }

                short numDistricts = reader.ReadInt16();
                Log.Debug($"m_nDistricts = {numDistricts}");
            }
        }
    }
}
