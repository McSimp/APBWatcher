using System;

namespace APBWatcher.Networking
{
    internal abstract class BasePacketHandler<TClient> : IPacketHandler where TClient : BaseClient
    {
        public abstract void HandlePacket(TClient client, ServerPacket packet);
         
        public void HandlePacket(BaseClient client, ServerPacket packet)
        {
            HandlePacket((TClient)client, packet);
        }
    }
}