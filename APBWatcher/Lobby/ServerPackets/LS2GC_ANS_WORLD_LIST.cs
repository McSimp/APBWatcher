using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using APBWatcher.Networking;

namespace APBWatcher.Lobby
{
    public partial class LobbyClient
    {
        [PacketHandler(LobbyOpCode.LS2GC_ANS_WORLD_LIST)]
        private class LS2GC_ANS_WORLD_LIST : BasePacketHandler<LobbyClient>
        {
            public override void HandlePacket(LobbyClient client, ServerPacket packet)
            {
                var reader = packet.Reader;

                int returnCode = reader.ReadInt32();
                if (returnCode != 0)
                {
                    Log.Error($"LS2GC_ANS_WORLD_LIST response had invalid return code {returnCode}");
                    client.OnGetWorldListFailed(client, returnCode);
                    return;
                }

                int numWorlds = reader.ReadInt16();
                var worlds = new List<WorldInfo>(numWorlds);

                for (int i = 0; i < numWorlds; i++)
                {
                    var info = new WorldInfo
                    {
                        UID = reader.ReadInt32(),
                        Name = reader.ReadUnicodeString(34),
                        Status = reader.ReadByte(),
                        Population = reader.ReadByte(),
                        EnfFaction = reader.ReadByte(),
                        CrimFaction = reader.ReadByte(),
                        PremiumOnly = reader.ReadByte(),
                        PingIP = new IPAddress(reader.ReadBytes(4))
                    };

                    worlds.Add(info);
                }

                client.OnGetWorldListSuccess(client, worlds);
            }
        }
    }
}
