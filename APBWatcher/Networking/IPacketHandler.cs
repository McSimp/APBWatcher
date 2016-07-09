namespace APBWatcher.Networking
{
    internal interface IPacketHandler
    {
        void HandlePacket(APBClient client, ServerPacket packet);
    }
}