using System.Collections.Generic;
using System.Threading;

namespace OrangeBugReloaded.Core.ClientServer
{
    public class ClientConnection
    {
        public string ConnectionId { get; }

        public string PlayerId { get; }

        public string PlayerDisplayName { get; }

        public HashSet<Point> LoadedChunks { get; } = new HashSet<Point>();

        public SemaphoreSlim MoveSemaphore { get; } = new SemaphoreSlim(1);

        public ClientConnection(string connectionId, ClientConnectRequest connectRequest)
        {
            ConnectionId = connectionId;
            PlayerId = connectRequest.PlayerId;
            PlayerDisplayName = connectRequest.PlayerDisplayName;
        }
    }
}
