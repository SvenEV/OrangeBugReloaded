using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer
{
    public sealed class RemoteChunkStorage : IChunkStorage
    {
        private string _connectionId;
        private IGameServer _server;
        private Dictionary<Point, IChunk> _loadedChunks = new Dictionary<Point, IChunk>();

        public RemoteChunkStorage(string connectionId, IGameServer server)
        {
            _connectionId = connectionId;
            _server = server;
        }

        public Task DeleteAsync(Point index)
        {
            throw new NotImplementedException();
        }

        public async Task<IChunk> LoadAsync(Point index, CancellationToken cancellation)
        {
            var chunk = _loadedChunks.TryGetValue(index);

            if (chunk == null)
            {
                chunk = await _server.LoadChunkAsync(_connectionId, index);
                _loadedChunks.Add(index, chunk);
            }

            return chunk;
        }

        public Task<IMapMetadata> LoadMetadataAsync()
        {
            throw new NotImplementedException();
        }

        public Task SaveAsync(Point index, IChunk chunk)
        {
            throw new NotImplementedException();
        }

        public Task SaveMetadataAsync(IMapMetadata metadata)
        {
            throw new NotImplementedException();
        }
    }
}
