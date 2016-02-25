using Newtonsoft.Json;

namespace OrangeBugReloaded.Core.ClientServer
{
    public struct GameClientInfo
    {
        public string PlayerId { get; }
        public string PlayerDisplayName { get; }

        [JsonConstructor]
        public GameClientInfo(string playerId, string playerDisplayName)
        {
            PlayerId = playerId;
            PlayerDisplayName = playerDisplayName;
        }
    }
}
