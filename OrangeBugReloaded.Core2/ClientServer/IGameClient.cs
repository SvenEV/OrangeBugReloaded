using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer
{
    public interface IGameClient : IGameClientStub
    {
        Point PlayerPosition { get; }

        string PlayerId { get; }

        string PlayerDisplayName { get; }

        IGameplayMap Map { get; }

        /// <summary>
        /// Connects to a game server.
        /// </summary>
        /// <param name="server">Server</param>
        /// <returns></returns>
        Task ConnectAsync(IGameServerStub server);

        /// <summary>
        /// Disconnects from a game server.
        /// </summary>
        /// <returns></returns>
        Task DisconnectAsync();

        Task<bool> MoveAsync(Point sourcePosition, Point targetPosition);

        Task<bool> MovePlayerAsync(Point direction);
    }
}
