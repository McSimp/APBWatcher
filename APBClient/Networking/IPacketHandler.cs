namespace APBClient.Networking
{
    internal interface IPacketHandler
    {
        void HandlePacket(BaseClient client, ServerPacket packet);
    }
}