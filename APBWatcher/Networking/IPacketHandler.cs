namespace APBWatcher.Networking
{
    internal interface IPacketHandler
    {
        void HandlePacket(BaseClient client, ServerPacket packet);
    }
}