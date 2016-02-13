using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer
{
    /// <summary>
    /// Hosts an Orange Bug Map and allows multiple players to
    /// connect and play.
    /// 
    /// TODO: How to get notified about transaction chain commits?
    ///       We have to forward the changes to the clients (which have loaded the affected chunks).
    /// </summary>
    public class GameServer : IGameServer
    {
        // Maps connection IDs to ClientInfos
        private Dictionary<string, ClientConnection> _connectedClients = new Dictionary<string, ClientConnection>();

        // Maps chunk indices to lists of client connections that have currently loaded that chunk
        private Dictionary<Point, List<ClientConnection>> _connectionsByChunk = new Dictionary<Point, List<ClientConnection>>();

        // Stores moves that must be executed in the future
        private SortedList<DateTimeOffset, ScheduledMove> _scheduledMoves = new SortedList<DateTimeOffset, ScheduledMove>();
        
        // The task that executes scheduled moves
        private readonly Task _updateTask;

        public IGameplayMap Map { get; }

        public GameServer(IGameplayMap map)
        {
            Map = map;
            _updateTask = RunAsync();
        }

        /// <inheritdoc/>
        public async Task<ConnectResult> ConnectAsync(ClientConnectRequest clientInfo)
        {
            if (_connectedClients.Any(client => client.Value.PlayerId == clientInfo.PlayerId))
                return new ConnectResult(false, null, Point.Zero, "Another client has already connected using the same player ID");

            var connection = new ClientConnection(Guid.NewGuid().ToString(), clientInfo);
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
                    _connectedClients.Add(connection.ConnectionId, connection);
                    return new ConnectResult(true, connection.ConnectionId, playerInfo.Position);
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
                    await spawnResult.Item1.Transaction.CommitAsync(Map, Map.Metadata.NextVersion(), null);                    
                    var playerInfo = new PlayerInfo(clientInfo.PlayerId, clientInfo.PlayerDisplayName, spawnResult.Item2);
                    Map.Metadata.Players.Add(playerInfo);
                    _connectedClients.Add(connection.ConnectionId, connection);
                    return new ConnectResult(true, connection.ConnectionId, spawnResult.Item2);
                }
                else
                {
                    // TODO: What now? We could not spawn the player!
                    throw new NotImplementedException();
                }
            }
        }

        /// <inheritdoc/>
        public async Task DisconnectAsync(string connectionId)
        {
            var connection = GetConnection(connectionId);

            // TODO: Remove PlayerEntity from Map (despawn)

            // Unload all the chunks currently loaded by the client
            foreach (var index in connection.LoadedChunks)
                await UnloadChunkAsync(connectionId, index);

            _connectedClients.Remove(connectionId);
        }

        /// <inheritdoc/>
        public async Task<IChunk> LoadChunkAsync(string connectionId, Point index)
        {
            var connection = GetConnection(connectionId);

            var chunk = await Map.ChunkLoader.GetAsync(index);

            var connectionsList = _connectionsByChunk.TryGetValue(index);
            if (connectionsList == null)
                connectionsList = _connectionsByChunk[index] = new List<ClientConnection>();

            connection.LoadedChunks.Add(index);
            connectionsList.Add(connection);
            return chunk.Clone();
        }

        /// <inheritdoc/>
        public Task UnloadChunkAsync(string connectionId, Point index)
        {
            var connection = GetConnection(connectionId);
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
        public async Task<RemoteMoveResult> MoveAsync(string connectionId, RemoteMoveRequest move)
        {
            // TODO: Test, test, test!

            var transaction = new TransactionWithMoveSupport(MoveInitiator.Empty);
            var moveResult = await Map.MoveAsync(move.SourcePosition, move.TargetPosition, transaction);

            var clientAffectedTiles = move.AffectedPositions;
            var serverAffectedTiles = moveResult.Transaction.Changes.Select(c => new VersionedPoint(c.Key, c.Value.Version)).ToList();

            var versions = serverAffectedTiles.FullOuterJoin(clientAffectedTiles,
                vp => vp.Position,
                vp => vp.Position,
                (serverVP, clientVP, p) => new { Position = p, ServerVersion = serverVP.Version, ClientVersion = clientVP.Version },
                VersionedPoint.Empty,
                VersionedPoint.Empty);

            if (versions.Any(v => v.ClientVersion > v.ServerVersion))
                throw new NotImplementedException("This should not happen. Clients must not have a newer version than the server");

            if (versions.Where(v => v.ClientVersion != -1 && v.ServerVersion != -1).All(v => v.ClientVersion == v.ServerVersion) &&
                clientAffectedTiles.Count != serverAffectedTiles.Count)
                throw new NotImplementedException("This should not happen. If client and server share the same versions for affected tiles, the sets of affected tiles must not differ");

            var conflictingVersions = versions.Where(v => v.ClientVersion < v.ServerVersion);

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
                if (moveResult.IsSuccessful)
                {
                    var newVersion = Map.Metadata.NextVersion();
                    await moveResult.Transaction.CommitAsync(Map, newVersion, null); // TODO: Emit events

                    foreach (var scheduledMove in moveResult.ScheduledMoves)
                        _scheduledMoves.Add(scheduledMove.ExecutionTime, scheduledMove);

                    return RemoteMoveResult.CreateSuccessful(newVersion);
                }
                else
                {
                    return RemoteMoveResult.CreateSuccessful(-1);
                }
            }
        }


        private async Task RunAsync()
        {
            while (true)
            {
                var now = DateTimeOffset.Now;

                // Run all scheduled moves that should have run until now
                while (_scheduledMoves.Any() && _scheduledMoves.First().Value.ExecutionTime <= now)
                {
                    var move = _scheduledMoves.First().Value;
                    _scheduledMoves.RemoveAt(0);

                    var transaction = new TransactionWithMoveSupport(move.Initiator);
                    var moveResult = await Map.MoveAsync(move.SourcePosition, move.TargetPosition, transaction);

                }

                await Task.Delay(100);
            }
        }

        private ClientConnection GetConnection(string connectionId)
        {
            var connection = _connectedClients.TryGetValue(connectionId);

            if (connection == null)
                throw new ArgumentException($"There is no connection with ID '{connectionId}'");

            return connection;
        }
    }
}
