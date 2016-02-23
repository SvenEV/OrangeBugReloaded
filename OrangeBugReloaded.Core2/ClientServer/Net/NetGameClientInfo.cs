namespace OrangeBugReloaded.Core.ClientServer.Net
{
    public class NetGameClientInfo : IGameClientInfo
    {
        public string ConnectionId { get; }

        public string PlayerDisplayName { get; }

        public string PlayerId { get; }

        public NetGameClientInfo(string connectionId, string playerId, string playerDisplayName)
        {
            ConnectionId = connectionId;
            PlayerId = playerId;
            PlayerDisplayName = playerDisplayName;
        }
    }
}
