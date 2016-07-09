using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using APBWatcher.Networking;

namespace APBWatcher.Lobby
{
    public partial class LobbyClient
    {
        [PacketHandler(LobbyOpCode.LS2GC_ANS_LOGIN_FAILED)]
        private class LS2GC_ANS_LOGIN_FAILED : BasePacketHandler<LobbyClient>
        {
            public override void HandlePacket(LobbyClient client, ServerPacket packet)
            {
                var reader = packet.Reader;

                var data = new LoginFailedData
                {
                    ReturnCode = reader.ReadUInt32(),
                    CountryCode = reader.ReadUnicodeString(48)
                };

                Log.Error($"Login failed: returnCode={data.ReturnCode}, countryCode={data.CountryCode}");

                client.OnLoginFailed(client, data);
                client.Disconnect();
            }
        }
    }
}
