namespace OrangeBugReloaded.Core.ClientServer.Local
{
    public class LocalGameServerInfo : IGameServerInfo
    {
        public LocalGameServer Server { get; }

        public LocalGameServerInfo(LocalGameServer server)
        {
            Server = server;
        }
    }
}
