﻿using System.Threading.Tasks;
using UwpNetworkingEssentials.Rpc;

namespace OrangeBugReloaded.Core.ClientServer.Net
{
    public class ClientSideServerProxy : IGameServerStub
    {
        private readonly RpcConnection _connection;

        public ClientSideServerProxy(RpcConnection connection)
        {
            _connection = connection;
        }

        public Task LeaveAsync(string playerId)
        {
            return _connection.Proxy.LeaveAsync();
        }

        public async Task<IChunk> LoadChunkAsync(Point index, string playerId)
        {
            // We must use await here, because RPC-calls always return Task<object>, but we need Task<IChunk>
            return await _connection.Proxy.LoadChunkAsync(index);
        }

        public async Task<RemoteMoveResult> MoveAsync(RemoteMoveRequest move, string playerId)
        {
            return await _connection.Proxy.MoveAsync(move);
        }

        public async Task<bool> ResetRegionAsync(string playerId)
        {
            return await _connection.Proxy.ResetRegionAsync();
        }

        public Task UnloadChunkAsync(Point index, string playerId)
        {
            return _connection.Proxy.UnloadChunkAsync(index);
        }
    }
}
