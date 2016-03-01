using OrangeBugReloaded.Core.Events;

namespace OrangeBugReloaded.Core.ClientServer
{
    public class PlayerJoinEvent : IGameEvent
    {
        public GameClientInfo PlayerInfo { get; }

        public PlayerJoinEvent(GameClientInfo playerInfo)
        {
            PlayerInfo = playerInfo;
        }
    }
}
