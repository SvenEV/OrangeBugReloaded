using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer
{
    /// <summary>
    /// Hosts an Orange Bug Map and allows multiple players to
    /// connect and play.
    /// </summary>
    public sealed class GameServer : IGameServerStub
    {
        // Maps connection IDs to ClientConnections
        private readonly Dictionary<string, ClientConnection> _clients = new Dictionary<string, ClientConnection>();

        // Maps chunk indices to lists of client connections that have currently loaded that chunk
        private readonly Dictionary<Point, List<ClientConnection>> _connectionsByChunk = new Dictionary<Point, List<ClientConnection>>();

        // Stores follow-up events that must be executed in the future
        private readonly List<FollowUpEvent> _scheduledFollowUpEvents = new List<FollowUpEvent>();

        // The task that executes scheduled follow-up events
        private readonly Task _updateTask;

        public IGameplayMap Map { get; }

        public GameServer(IGameplayMap map)
        {
            Map = map;
            map.ChunkLoader.Chunks.ItemAdded += OnChunkLoaded;
            _updateTask = RunAsync();
        }

        private async void OnChunkLoaded(KeyValuePair<Point, IChunk> kvp)
        {
            // TODO: Decide to what extent chunk loading should be
            //       handled in Map and in GameServer.

            // Properly initialize the chunk.
            // Example scenarios: On chunk loading...
            // - a button should be activated immediately if there's an entity pressing it
            // - a piston should extend immediately if its trigger is on
            // - a balloon should be popped immediately if it is on a pin
            var chunkPoints = new Rectangle(kvp.Key.X * Chunk.Size, kvp.Key.Y * Chunk.Size, Chunk.Size - 1, Chunk.Size - 1);
            var transaction = new TransactionWithMoveSupport(MoveInitiator.Empty);
            var followUpEvents = await ((Map)Map).UpdateTilesAsync(chunkPoints, transaction);
            await CommitAndBroadcastAsync(transaction, followUpEvents);
        }

        /// <inheritdoc/>
        public async Task<JoinResult> JoinAsync(GameClientInfo clientInfo, IGameClientStub clientStub)
        {
            if (_clients.Values.Any(conn => conn.ClientInfo.PlayerId == clientInfo.PlayerId.ToString()))
                return new JoinResult(false, Point.Zero, "Another client is already connected using the same player ID");

            var connection = new ClientConnection(clientInfo, clientStub);
            var playerEntity = new PlayerEntity(clientInfo.PlayerId, Point.North);

            // Check if the player is playing this map the first time
            if (Map.Metadata.Players.IsKnown(clientInfo.PlayerId))
            {
                // Try to spawn player at its last known position.
                var playerInfo = Map.Metadata.Players[clientInfo.PlayerId];
                var spawnResult = await Map.SpawnAsync(playerEntity, playerInfo.Position);

                if (spawnResult.IsSuccessful)
                {
                    // TODO: What if a player spawns within a level some others are currently trying to solve?
                    await CommitAndBroadcastAsync(spawnResult.Transaction, spawnResult.FollowUpEvents, connection);
                    _clients.Add(clientInfo.PlayerId, connection);
                    return new JoinResult(true, playerInfo.Position);
                }
                else
                {
                    // If that fails,
                    // option 1: spawn at global spawn area
                    // option 2: spawn at regional spawn area, if that fails try parent region etc.
                    throw new NotImplementedException();
                }
            }
            else
            {
                // Unknown player: Spawn player somewhere in global spawn area, add to list of known players
                var spawnResult = await Map.SpawnAsync(playerEntity, Map.Metadata.RootRegion.SpawnArea);

                if (spawnResult != null)
                {
                    await CommitAndBroadcastAsync(spawnResult.Transaction, spawnResult.FollowUpEvents, connection);
                    var playerInfo = new PlayerInfo(clientInfo.PlayerId, clientInfo.PlayerDisplayName, spawnResult.SpawnPosition);
                    Map.Metadata.Players.Add(playerInfo);
                    _clients.Add(clientInfo.PlayerId, connection);
                    return new JoinResult(true, spawnResult.SpawnPosition);
                }
                else
                {
                    // TODO: What now? We could not spawn the player!
                    throw new NotImplementedException();
                }
            }
        }

        /// <inheritdoc/>
        public async Task LeaveAsync(string playerId)
        {
            var connection = GetClient(playerId);
            var player = Map.Metadata.Players[playerId];

            // Remove PlayerEntity from Map (despawn)
            var despawnResult = await Map.DespawnAsync(player.Position);

            if (despawnResult.IsSuccessful)
            {
                await CommitAndBroadcastAsync(despawnResult.Transaction, despawnResult.FollowUpEvents, connection);

                // Unload all the chunks currently loaded by the client
                while (connection.LoadedChunks.Any())
                    await UnloadChunkAsync(connection.LoadedChunks.First(), playerId);

                _clients.Remove(playerId);
            }
            else
            {
                // TODO: What now? We could not despawn the player!
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public async Task<IChunk> LoadChunkAsync(Point index, string playerId)
        {
            var connection = GetClient(playerId);

            var chunk = await Map.ChunkLoader.GetAsync(index);

            var connectionsList = _connectionsByChunk.TryGetValue(index);
            if (connectionsList == null)
                connectionsList = _connectionsByChunk[index] = new List<ClientConnection>();

            connection.LoadedChunks.Add(index);
            connectionsList.Add(connection);
            return chunk.Clone();
        }

        /// <inheritdoc/>
        public Task UnloadChunkAsync(Point index, string playerId)
        {
            var connection = GetClient(playerId);
            connection.LoadedChunks.Remove(index);

            var connectionsList = _connectionsByChunk.TryGetValue(index);

            if (connectionsList != null)
            {
                connectionsList.Remove(connection);

                if (!connectionsList.Any())
                {
                    // No more clients have loaded the chunk
                    // TODO: Check if we can unload the chunk on the server
                    // We can unload chunks which are no longer referenced by clients or by dependencies from other chunks
                    // (Finding these chunks again involves checking dependencies and determining a topological order of checking)
                    _connectionsByChunk.Remove(index);
                }
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<RemoteMoveResult> MoveAsync(RemoteMoveRequest move, string playerId)
        {
            // TODO: Test, test, test!

            var connection = GetClient(playerId);
            await connection.MoveSemaphore.WaitAsync();

            try
            {
                var source = await Map.GetAsync(move.SourcePosition);
                var target = await Map.GetAsync(move.TargetPosition);
                var initiator = new MoveInitiator(source.Tile.Entity, move.SourcePosition);
                var transaction = new TransactionWithMoveSupport(initiator);
                var moveResult = await Map.MoveAsync(move.SourcePosition, move.TargetPosition, transaction);

                // Compare affected tiles of the client and those of the server
                var clientAffectedTiles = move.AffectedPositions.Union(new[] { move.SourcePosition, move.TargetPosition });
                var serverAffectedTiles = moveResult.Transaction.Changes
                    .Select(c => new VersionedPoint(c.Key, c.Value.Version))
                    .Union(new[] { new VersionedPoint(move.SourcePosition, source.Version), new VersionedPoint(move.TargetPosition, target.Version) })
                    .ToList();

                var versions = serverAffectedTiles.FullOuterJoin(clientAffectedTiles,
                    vp => vp.Position,
                    vp => vp.Position,
                    (serverVP, clientVP, p) => new { Position = p, ServerVersion = serverVP.Version, ClientVersion = clientVP.Version },
                    VersionedPoint.Empty,
                    VersionedPoint.Empty);

                if (versions.Any(v => v.ClientVersion != -1 && v.ServerVersion != -1 && v.ClientVersion > v.ServerVersion))
                    throw new NotImplementedException("This should not happen. Clients must not have a newer version than the server");

                var conflictingVersions = versions.Where(v =>
                    (v.ClientVersion == -1 && v.ServerVersion != -1) ||
                    (v.ClientVersion != -1 && v.ServerVersion == -1) ||
                    (v.ClientVersion != -1 && v.ServerVersion != -1 && v.ClientVersion < v.ServerVersion));

                if (conflictingVersions.Any())
                {
                    // Client not up to date => Cancel move request and send up-to-date chunks
                    var chunks = await Task.WhenAll(conflictingVersions
                        .Select(v => v.Position / Chunk.Size).Distinct()
                        .Select(async index => new KeyValuePair<Point, IChunk>(index, await Map.ChunkLoader.GetAsync(index))));

                    return RemoteMoveResult.CreateFaulted(chunks);
                }
                else
                {
                    // Client and server are on the same version (regarding affected tiles)
                    // => Commit and schedule follow-up transactions
                    // => Notify other clients about the changes

                    var newVersion = await CommitAndBroadcastAsync(moveResult.Transaction, moveResult.FollowUpEvents, connection);
                    return RemoteMoveResult.CreateSuccessful(newVersion);
                }
            }
            finally
            {
                connection.MoveSemaphore.Release();
            }
        }

        private async Task RunAsync()
        {
            while (true)
            {
                var now = DateTimeOffset.Now;

                // Run all scheduled moves that should have run until now
                while (_scheduledFollowUpEvents.Any() && _scheduledFollowUpEvents.First().ExecutionTime <= now)
                {
                    // Get the "oldest" follow-up event
                    var followUpEvent = _scheduledFollowUpEvents.OrderBy(e => e.ExecutionTime).First();
                    _scheduledFollowUpEvents.Remove(followUpEvent);

                    var tileInfo = await Map.GetAsync(followUpEvent.Position);

                    // Trigger follow-up transactions
                    // e.g. a teleporter might use this to teleport an entity
                    // (which can result in further follow-up events).
                    var transaction = new TransactionWithMoveSupport(followUpEvent.Initiator);
                    var args = new GameplayArgs(transaction, Map) as IFollowUpArgs;
                    await tileInfo.Tile.OnFollowUpTransactionAsync(args, followUpEvent.Position);
                    await CommitAndBroadcastAsync(transaction, args.FollowUpEvents);
                }

                await Task.Delay(200);
            }
        }

        private async Task<int> CommitAndBroadcastAsync(ITransactionWithMoveSupport transaction, IEnumerable<FollowUpEvent> followUpEvents, ClientConnection excludedConnection = null)
        {
            var newVersion = -1;

            // TODO: Emit events
            if (transaction.Changes.Any())
            {
                // Get new version number for the tiles that changed
                newVersion = Map.Metadata.NextVersion();

                // Apply changes to the server's map (using the version number above)
                await transaction.CommitAsync(Map, newVersion, null);

                // Send the updated tiles to clients that have loaded the corresponding map area
                await BroadcastChangesAsync(transaction, newVersion, excludedConnection);
            }

            // Schedule follow-up events that might have been created during e.g. a move
            _scheduledFollowUpEvents.AddRange(followUpEvents);

            return newVersion;
        }

        /// <summary>
        /// Sends a subset of the changes and events of the specified transaction to the
        /// clients depending on which chunks each client has currently loaded.
        /// </summary>
        /// <param name="transaction">The transaction that contains the changes that are broadcasted</param>
        /// <param name="newVersion">The version to be used for the updates tiles</param>
        /// <param name="excludedConnection">An optional connection that is excluded and does not receive the tile updates</param>
        /// <returns></returns>
        private async Task BroadcastChangesAsync(ITransactionWithMoveSupport transaction, int newVersion, ClientConnection excludedConnection = null)
        {
            // Check which players have moved and update their positions in map metadata
            var playerMoveEvents = transaction.Events
                .OfType<EntityMoveEvent>()
                .Where(e => e.Source.Entity is PlayerEntity)
                .Select(e => new { PlayerId = (e.Source.Entity as PlayerEntity)?.PlayerId, NewPosition = e.TargetPosition });

            foreach (var move in playerMoveEvents)
                Map.Metadata.Players[move.PlayerId] =
                    Map.Metadata.Players[move.PlayerId].WithPosition(move.NewPosition);

            // Notify clients that have loaded the affected chunks
            foreach (var conn in _clients.Values)
            {
                if (conn == excludedConnection)
                    continue; // Skip calling client

                var relevantChanges = transaction.Changes
                    .Where(change => conn.LoadedChunks.Contains(change.Key / Chunk.Size))
                    .Select(change => new TileUpdate(change.Key, change.Value.WithVersion(newVersion)))
                    .ToArray();

                var relevantEvents = transaction.Events
                    .OfType<ILocatedGameEvent>()
                    .Where(ev => ev.GetPositions().Any(p => conn.LoadedChunks.Contains(p / Chunk.Size)))
                    .ToArray();

                if (relevantChanges.Any() || relevantEvents.Any())
                {
                    // If there are any tile changes or any events that are of interest
                    // to the client, push them to the client.
                    var update = new ClientUpdate(relevantChanges, relevantEvents);
                    await conn.ClientStub.OnUpdate(update);
                }
            }
        }

        private ClientConnection GetClient(string playerId)
        {
            var client = _clients.TryGetValue(playerId.ToString());

            if (client == null)
                throw new ArgumentException($"There is no client with player ID '{playerId.ToString()}'");

            return client;
        }
    }
}
