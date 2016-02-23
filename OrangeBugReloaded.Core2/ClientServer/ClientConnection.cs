using System.Collections.Generic;
using System.Threading;

namespace OrangeBugReloaded.Core.ClientServer
{
    public class ClientConnection
    {
        public string PlayerId { get; }

        public IGameClientStub Client { get; }
        
        public HashSet<Point> LoadedChunks { get; } = new HashSet<Point>();

        public SemaphoreSlim MoveSemaphore { get; } = new SemaphoreSlim(1);

        public ClientConnection(string playerId, IGameClientStub client)
        {
            Client = client;
            PlayerId = playerId;
        }
    }
}
