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
        [PacketHandler(LobbyOpCode.LS2GC_ANS_WORLD_ENTER)]
        private class LS2GC_ANS_WORLD_ENTER : BasePacketHandler<LobbyClient>
        {
            public override void HandlePacket(LobbyClient client, ServerPacket packet)
            {
                var reader = packet.Reader;

                int returnCode = reader.ReadInt32();
                if (returnCode != 0)
                {
                    Log.Error($"LS2GC_ANS_WORLD_ENTER response had invalid return code {returnCode}");
                    client.OnWorldEnterFailed(client, returnCode);
                    return;
                }

                var worldServerIp = new IPAddress(reader.ReadBytes(4));
                ushort worldServerPort = reader.ReadUInt16();
                ulong timestamp = reader.ReadUInt64();
                var pingServerIp = new IPAddress(reader.ReadBytes(4));

                Log.Debug($"m_nReturnCode = {returnCode}");
                Log.Debug($"m_nWorldServerIPAddress = {worldServerIp}");
                Log.Debug($"m_nWorldServerPingIPAddress = {pingServerIp}");
                Log.Debug($"m_nWorldServerPort = {worldServerPort}");
                Log.Debug($"m_nTimestamp = {timestamp}");

                client.OnWorldEnterSuccess(client, null);
            }
        }
    }
}
