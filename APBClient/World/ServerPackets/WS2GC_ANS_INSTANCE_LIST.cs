using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using APBClient.Networking;

namespace APBClient.World
{
    public partial class WorldClient
    {
        [PacketHandler(APBOpCode.WS2GC_ANS_INSTANCE_LIST)]
        private class WS2GC_ANS_INSTANCE_LIST : BasePacketHandler<WorldClient>
        {
            public override void HandlePacket(WorldClient client, ServerPacket packet)
            {
                var reader = packet.Reader;

                int returnCode = reader.ReadInt32();
                if (returnCode != 0)
                {
                    Log.Error($"WS2GC_ANS_INSTANCE_LIST response had invalid return code {returnCode}");
                    client.OnInstanceListFailed(client, returnCode);
                    return;
                }

                short numInstances = reader.ReadInt16();

                var instances = new List<InstanceInfo>(numInstances);
                for (int i = 0; i < numInstances; i++)
                {
                    int temp = reader.ReadInt32();

                    var instance = new InstanceInfo
                    {
                        DistrictUid = temp << 8 >> 8,
                        InstanceNum = temp >> 24,
                        Enforcers = reader.ReadInt16(),
                        Criminals = reader.ReadInt16(),
                        DistrictStatus = reader.ReadByte(),
                        QueueSize = reader.ReadInt16(),
                        Threat = reader.ReadInt32()
                    };

                    instances.Add(instance);
                }

                client.OnInstanceListSuccess(client, instances);
            }
        }
    }
}
