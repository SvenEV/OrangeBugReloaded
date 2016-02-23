using System;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Rpc;

namespace OrangeBugReloaded.Core.ClientServer.Net.Client
{
    public class ClientSideServerProxy : IGameServerStub
    {
        private readonly RpcConnection _connection;

        public ClientSideServerProxy(RpcConnection connection)
        {
            _connection = connection;
        }

        public async Task<ConnectResult> ConnectAsync(IGameClientStub client, object playerId)
        {
            var clientInfo = client as ClientConnectInfo;

            if (clientInfo == null)
                throw new ArgumentException($"{nameof(ClientConnectInfo)} expected", nameof(client));

            return await _connection.Proxy.ConnectAsync(client);
        }

        public Task DisconnectAsync(object playerId)
        {
            return _connection.Proxy.DisconnectAsync();
        }

        public Task<IChunk> LoadChunkAsync(Point index, object playerId)
        {
            return _connection.Proxy.LoadChunkAsync(index);
        }

        public Task<RemoteMoveResult> MoveAsync(RemoteMoveRequest move, object playerId)
        {
            return _connection.Proxy.MoveAsync(move);
        }

        public Task UnloadChunkAsync(Point index, object playerId)
        {
            return _connection.Proxy.UnloadChunkAsync(index);
        }
    }
}
