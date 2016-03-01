using OrangeBugReloaded.Core.Events;

namespace OrangeBugReloaded.Core.ClientServer
{
    public class PlayerLeaveEvent : IGameEvent
    {
        public GameClientInfo PlayerInfo { get; }

        public PlayerLeaveEvent(GameClientInfo playerInfo)
        {
            PlayerInfo = playerInfo;
        }
    }
}
