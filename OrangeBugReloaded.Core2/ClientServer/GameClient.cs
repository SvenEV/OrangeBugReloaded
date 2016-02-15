﻿using OrangeBugReloaded.Core.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer
{
    public class GameClient : IGameClient
    {
        private IGameServer _server;
        private string _connectionId;

        public Point PlayerPosition { get; private set; }
        public string PlayerId { get; }
        public string PlayerDisplayName { get; }

        public IGameplayMap Map { get; private set; }

        public GameClient(string playerId, string playerDisplayName)
        {
            PlayerId = playerId;
            PlayerDisplayName = playerDisplayName;
        }

        public async Task ConnectAsync(IGameServer server)
        {
            _server = server;

            var request = new ClientConnectRequest(PlayerId, PlayerDisplayName);
            var result = await _server.ConnectAsync(this);

            if (result.IsSuccessful)
            {
                _connectionId = result.ConnectionId;
                PlayerPosition = result.SpawnPosition;

                Map = new Map(new RemoteChunkStorage(result.ConnectionId, _server));

                // Load chunk at spawn position
                await Map.GetAsync(result.SpawnPosition);
            }
            else
            {
                Debugger.Break();
                throw new Exception(result.Message);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_server == null)
                return;

            await _server.DisconnectAsync(_connectionId);
            _server = null;
            _connectionId = null;
            PlayerPosition = Point.Zero;
            Map = null;
        }

        public async Task<bool> MoveAsync(Point sourcePosition, Point targetPosition)
        {
            var transaction = new TransactionWithMoveSupport(MoveInitiator.Empty);
            var result = await Map.MoveAsync(sourcePosition, targetPosition, transaction);

            var source = await Map.GetAsync(sourcePosition);
            var target = await Map.GetAsync(targetPosition);

            var request = result.IsSuccessful ?
                RemoteMoveRequest.CreateSuccessful(
                    new VersionedPoint(sourcePosition, source.Version),
                    new VersionedPoint(targetPosition, target.Version),
                    result.Transaction.Changes.Select(c => new VersionedPoint(c.Key, c.Value.Version))) :
                RemoteMoveRequest.CreateFaulted(
                    new VersionedPoint(sourcePosition, source.Version),
                    new VersionedPoint(targetPosition, target.Version));

            var remoteMoveResult = await _server.MoveAsync(_connectionId, request);

            if (remoteMoveResult.IsSuccessful)
            {
                // Commit transaction
                if (!transaction.IsCanceled)
                {
                    await transaction.CommitAsync(Map, remoteMoveResult.NewVersion, null);
                    return true;
                }
            }
            else
            {
                // Our map was not up to date => apply updated chunks
                foreach (var kvp in remoteMoveResult.ChunkUpdates)
                    await ApplyChunkAsync(kvp.Value, kvp.Key);
            }

            return false;
        }

        public async Task<bool> MovePlayerAsync(Point direction)
        {
            direction.EnsureDirection();
            var isSuccessful = await MoveAsync(PlayerPosition, PlayerPosition + direction);

            if (isSuccessful)
            {
                PlayerPosition = PlayerPosition + direction;
                return true;
            }

            return false;
        }

        private async Task ApplyChunkAsync(IChunk chunk, Point index)
        {
            // TODO: It would be better to directly replace the chunk via ChunkLoader
            for (var y = 0; y < Chunk.Size; y++)
            {
                for (var x = 0; x < Chunk.Size; x++)
                {
                    var position = index * Chunk.Size + new Point(x, y);
                    await Map.SetAsync(position, chunk[x, y]);
                    await Map.SetMetadataAsync(position, chunk.GetMetadata(new Point(x, y)));
                }
            }
        }

        public async Task OnTileUpdates(IEnumerable<TileUpdate> tileUpdates)
        {
            foreach (var change in tileUpdates)
                await Map.SetAsync(change.Position, change.TileInfo);
        }
    }
}