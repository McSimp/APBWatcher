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
        [PacketHandler(APBOpCode.LS2GC_ANS_WORLD_ENTER)]
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

                var data = new WorldEnterData()
                {
                    ReturnCode = returnCode,
                    WorldServerIpAddress = new IPAddress(reader.ReadBytes(4)),
                    WorldServerPort = reader.ReadUInt16(),
                    Timestamp = reader.ReadUInt64(),
                    PingServerIpAddress = new IPAddress(reader.ReadBytes(4))
                };

                Log.Debug($"m_nReturnCode = {returnCode}");
                Log.Debug($"m_nWorldServerIPAddress = {data.WorldServerIpAddress}");
                Log.Debug($"m_nWorldServerPingIPAddress = {data.PingServerIpAddress}");
                Log.Debug($"m_nWorldServerPort = {data.WorldServerPort}");
                Log.Debug($"m_nTimestamp = {data.Timestamp}");

                client.OnWorldEnterSuccess(client, data);
            }
        }
    }
}
