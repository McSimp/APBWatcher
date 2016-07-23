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
        [PacketHandler(APBOpCode.LS2GC_ERROR)]
        private class LS2GC_ERROR : BasePacketHandler<LobbyClient>
        {
            public override void HandlePacket(LobbyClient client, ServerPacket packet)
            {
                var reader = packet.Reader;

                var data = new ErrorData
                {
                    MessageId = reader.ReadUInt32(),
                    QueryId = reader.ReadUInt16(),
                    ReturnCode = reader.ReadUInt32(),
                    Param1 = reader.ReadUInt32(),
                    Param2 = reader.ReadUInt32(),
                    Param3 = reader.ReadUInt32(),
                    Param4 = reader.ReadUInt32()
                };

                Log.Error($"An error occurred with interacting with the Lobby server: messageId={data.MessageId}, queryId={data.QueryId}, returnCode={data.ReturnCode}, param1={data.Param1}, param2={data.Param2}, param3={data.Param3}, param4={data.Param4}");

                client.OnError(client, data);
                client.Disconnect();
            }
        }
    }
}
