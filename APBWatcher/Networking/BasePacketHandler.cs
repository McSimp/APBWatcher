using System;

namespace APBWatcher.Networking
{
    internal abstract class BasePacketHandler<TClient> : IPacketHandler where TClient : APBClient
    {
        public abstract void HandlePacket(TClient client, ServerPacket packet);
         
        public void HandlePacket(APBClient client, ServerPacket packet)
        {
            HandlePacket((TClient)client, packet);
        }
    }
}