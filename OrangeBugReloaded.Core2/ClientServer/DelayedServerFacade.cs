using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer
{
    /// <summary>
    /// A GameServer for testing and debugging purposes
    /// that adds artifical delay between calls.
    /// </summary>
    public class DelayedServerFacade : IGameServer
    {
        private const int _connectDelay = 500;
        private const int _disconnectDelay = 500;
        private const int _moveDelay = 250;
        private const int _loadChunkDelay = 200;
        private const int _unloadChunkDelay = 200;

        private GameServer _server;

        public IGameplayMap Map => _server.Map;

        public DelayedServerFacade(GameServer server)
        {
            _server = server;
        }

        public async Task<ConnectResult> ConnectAsync(ClientConnectRequest clientInfo)
        {
            await Task.Delay(_connectDelay);
            var result = await _server.ConnectAsync(clientInfo);
            await Task.Delay(_connectDelay);
            return result;
        }

        public async Task DisconnectAsync(string connectionId)
        {
            await Task.Delay(_disconnectDelay);
            await _server.DisconnectAsync(connectionId);
        }

        public async Task<IChunk> LoadChunkAsync(string connectionId, Point index)
        {
            await Task.Delay(_loadChunkDelay);
            var result = await _server.LoadChunkAsync(connectionId, index);
            await Task.Delay(_loadChunkDelay);
            return result;
        }

        public async Task<RemoteMoveResult> MoveAsync(string connectionId, RemoteMoveRequest move)
        {
            await Task.Delay(_moveDelay);
            var result =  await _server.MoveAsync(connectionId, move);
            await Task.Delay(_moveDelay);
            return result;
        }

        public async Task UnloadChunkAsync(string connectionId, Point index)
        {
            await Task.Delay(_unloadChunkDelay);
            await _server.UnloadChunkAsync(connectionId, index);
        }
    }
}
