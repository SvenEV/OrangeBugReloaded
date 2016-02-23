﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer
{
    public sealed class RemoteChunkStorage : IChunkStorage
    {
        private string _playerId;
        private IGameServerStub _server;
        private Dictionary<Point, IChunk> _loadedChunks = new Dictionary<Point, IChunk>();

        public RemoteChunkStorage(string playerId, IGameServerStub server)
        {
            _playerId = playerId;
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
                chunk = await _server.LoadChunkAsync(index, _playerId);
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
