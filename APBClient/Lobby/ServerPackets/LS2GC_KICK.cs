using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using APBClient.Networking;

namespace APBClient.Lobby
{
    public partial class LobbyClient
    {
        [PacketHandler(APBOpCode.LS2GC_KICK)]
        private class LS2GC_KICK : BasePacketHandler<LobbyClient>
        {
            public override void HandlePacket(LobbyClient client, ServerPacket packet)
            {
                var reader = packet.Reader;
                var data = new KickData
                {
                    Reason = reader.ReadUInt32(),
                    Information = reader.ReadUnicodeString()
                };

                Log.Debug($"m_nReason = {data.Reason}");
                Log.Debug($"m_szInformation = {data.Information}");

                client.OnKick(client, data);
                client.Disconnect();
            }
        }
    }
}
