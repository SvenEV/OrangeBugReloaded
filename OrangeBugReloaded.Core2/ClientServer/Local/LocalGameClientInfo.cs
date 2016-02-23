namespace OrangeBugReloaded.Core.ClientServer.Local
{
    public class LocalGameClientInfo : IGameClientInfo
    {
        public LocalGameClient Client { get; }

        public string PlayerDisplayName { get; }

        public string PlayerId { get; }

        public LocalGameClientInfo(LocalGameClient client, string playerId, string playerDisplayName)
        {
            Client = client;
            PlayerId = playerId;
            PlayerDisplayName = playerDisplayName;
        }
    }
}
