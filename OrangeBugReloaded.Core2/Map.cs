using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    public class Map : IGameplayMap
    {
        private ISubject<IGameEvent> _eventSource;

        /// <inheritdoc/>
        public IMapMetadata Metadata { get; }

        /// <inheritdoc/>
        public IObservable<IGameEvent> Events => _eventSource;

        /// <inheritdoc/>
        public MapDependencyTable Dependencies { get; } = new MapDependencyTable();

        /// <inheritdoc/>
        public ChunkLoader ChunkLoader { get; }

        /// <summary>
        /// Initializes a new <see cref="Map"/> using the specified
        /// <see cref="IChunkStorage"/> to load and save chunks.
        /// </summary>
        /// <param name="storage">The chunk storage</param>
        public Map(IChunkStorage storage)
        {
            ChunkLoader = new ChunkLoader(storage);
            ChunkLoader.Chunks.ItemAdded += OnChunkLoaded;
            ChunkLoader.Chunks.ItemRemoved += OnChunkUnloaded;

            _eventSource = new Subject<IGameEvent>();

            // TODO: LoadMetadataAsync() should be treated like async!
            Metadata = storage.LoadMetadataAsync().Result;
        }

        /// <inheritdoc/>
        public void Emit(IGameEvent e)
        {
            _eventSource.OnNext(e);
        }

        /// <inheritdoc/>
        public async Task<TileInfo> GetAsync(Point position)
        {
            var chunk = await ChunkLoader.GetAsync(position / Chunk.Size);
            return chunk[position % Chunk.Size];
        }

        /// <inheritdoc/>
        public async Task<TileMetadata> GetMetadataAsync(Point position)
        {
            var chunk = await ChunkLoader.GetAsync(position / Chunk.Size);
            return chunk.GetMetadata(position % Chunk.Size);
        }

        /// <inheritdoc/>
        public async Task<bool> SetAsync(Point position, TileInfo tileInfo)
        {
            // TODO: Handle concurrency
            var chunk = await ChunkLoader.GetAsync(position / Chunk.Size);
            var oldTileInfo = chunk[position % Chunk.Size];

            if (!Equals(oldTileInfo, tileInfo))
            {
                // Update chunk
                chunk[position % Chunk.Size] = tileInfo;

                // Update dependencies
                Dependencies.RemoveDependenciesOf(oldTileInfo.Tile, position);
                Dependencies.AddDependenciesOf(tileInfo.Tile, position);

                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> SetMetadataAsync(Point position, TileMetadata value)
        {
            // TODO: Handle concurrency
            var chunk = await ChunkLoader.GetAsync(position / Chunk.Size);
            var oldMetadata = chunk.GetMetadata(position % Chunk.Size);

            if (!Equals(oldMetadata, value))
            {
                // Update chunk
                chunk.SetMetadata(position % Chunk.Size, value);
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<MoveResult> MoveAsync(Point sourcePosition, Point targetPosition, ITransactionWithMoveSupport transaction)
        {
            // Execute the actual move.
            // Any changes to the map are "recorded" in the transaction
            // and can later be applied to the map via a commit.
            var isSuccessful = await MoveCoreAsync(sourcePosition, targetPosition, transaction);

            // At the end of the recursion...
            if (transaction.Moves.Count == 0)
            {
                // Notify changed tiles and tiles that directly or indirectly depend on changed tiles
                var affectedTiles = transaction.Changes.Select(kvp => kvp.Key);
                var followUpEvents = await UpdateTilesAsync(affectedTiles, transaction);
                return new MoveResult(transaction, isSuccessful, followUpEvents);
            }

            return new MoveResult(transaction, isSuccessful, Enumerable.Empty<FollowUpEvent>());
        }

        /// <inheritdoc/>
        public async Task<MoveResult> SpawnAsync(Entity entity, Point position, ITransactionWithMoveSupport transaction)
        {
            var tileInfo = await GetAsync(position);
            tileInfo.Tile.EnsureNotNull();

            transaction.Moves.Push(new EntityMoveInfo(entity, position, position));

            // Try to attach entity
            var attachArgs = new GameplayArgs(transaction, this);
            await tileInfo.Tile.AttachEntityAsync(attachArgs);
            attachArgs.ValidateResult();

            if (transaction.IsSealed)
                return new MoveResult(transaction, false, Enumerable.Empty<FollowUpEvent>());

            var newTileInfo = tileInfo.WithTile(attachArgs.Result);
            transaction.Set(position, tileInfo, newTileInfo);
            transaction.Emit(new EntitySpawnEvent(position, entity));

            // Update tile
            var followUpEvents = await UpdateTilesAsync(new[] { position }, transaction);

            return new MoveResult(transaction, true, followUpEvents);
        }

        /// <inheritdoc/>
        public async Task<MoveResult> DespawnAsync(Point position, ITransactionWithMoveSupport transaction)
        {
            var tileInfo = await GetAsync(position);
            tileInfo.Tile.EnsureNotNull();

            // No entity => nothing to despawn
            if (tileInfo.Tile.Entity == Entity.None)
                return new MoveResult(transaction, true, Enumerable.Empty<FollowUpEvent>());

            transaction.Moves.Push(new EntityMoveInfo(tileInfo.Tile.Entity, position, position));

            // Try to detach entity
            var detachArgs = new GameplayArgs(transaction, this);
            await tileInfo.Tile.DetachEntityAsync(detachArgs);
            detachArgs.ValidateResult();

            if (transaction.IsSealed)
                return new MoveResult(transaction, false, Enumerable.Empty<FollowUpEvent>());

            var newTileInfo = tileInfo.WithTile(detachArgs.Result);
            transaction.Set(position, tileInfo, newTileInfo);
            transaction.Emit(new EntityDespawnEvent(position, tileInfo.Tile.Entity));

            // Update tile
            var followUpEvents = await UpdateTilesAsync(new[] { position }, transaction);

            return new MoveResult(transaction, true, followUpEvents);
        }

        /// <inheritdoc/>
        public async Task<ResetResult> ResetRegionAsync(Point position, ITransactionWithMoveSupport transaction)
        {
            // TOOD: Do not modify map directly, collect changes in a transaction
            // instead of 'bool' return 'ResetResult' containing the transaction

            var tileMeta = await GetMetadataAsync(position);
            var region = Metadata.Regions[tileMeta.RegionId];
            var regionPoints = await this.GetCoherentPositionsAsync(position);
            var playersInRegion = new List<PlayerEntity>();

            // Despawn all players within region
            foreach (var p in regionPoints)
            {
                var tileInfo = await GetAsync(p);
                var player = tileInfo.Tile.Entity as PlayerEntity;

                if (player != null)
                {
                    playersInRegion.Add(player);
                    var despawnResult = await DespawnAsync(p, transaction);
                    if (!despawnResult.IsSuccessful)
                        return new ResetResult(transaction, false, Enumerable.Empty<FollowUpEvent>()); // If we can't despawn a player in the region, we can't reset
                }
            }

            // Replace tiles with their templates
            foreach (var p in regionPoints)
            {
                var meta = await GetMetadataAsync(p);
                var oldTileInfo = await GetAsync(p);
                var newTileInfo = new TileInfo(meta.TileTemplate, oldTileInfo.Version); // Version will be set during commit

                var hasChanged = transaction.Set(p, oldTileInfo, newTileInfo);

                if (hasChanged && oldTileInfo.Tile.Entity != Entity.None)
                    transaction.Emit(new EntityDespawnEvent(p, oldTileInfo.Tile.Entity));

                if (hasChanged && newTileInfo.Tile.Entity != Entity.None)
                    transaction.Emit(new EntitySpawnEvent(p, newTileInfo.Tile.Entity));
            }

            // Respawn players at spawn position
            var spawnPoints = await this.GetCoherentPositionsAsync(region.SpawnPosition);
            var availableSpawnPoints = spawnPoints.Shuffle().ToQueue();

            if (availableSpawnPoints.Count < playersInRegion.Count)
                return new ResetResult(transaction, false, Enumerable.Empty<FollowUpEvent>()); // Spawn region is too small to respawn all players

            foreach (var player in playersInRegion)
            {
                var success = false;
                var spawnPosition = Point.Zero;

                while (!success && availableSpawnPoints.Any())
                {
                    transaction.IsSealed = false; // If the last spawn failed the transaction is sealed which would prevent any other spawns
                    spawnPosition = availableSpawnPoints.Dequeue();
                    var spawnResult = await SpawnAsync(player, spawnPosition, transaction);
                    success = spawnResult.IsSuccessful;
                }

                if (success)
                {
                    // Update player position in map metadata
                    Metadata.Players[player.PlayerId] = Metadata.Players[player.PlayerId].WithPosition(spawnPosition);
                }
                else
                {
                    // We ran out of available spawn positions and could not spawn the player
                    // => we can't fully reset the region
                    return new ResetResult(transaction, false, Enumerable.Empty<FollowUpEvent>());
                }
            }

            // Notify changed tiles and tiles that directly or indirectly depend on changed tiles
            var affectedTiles = transaction.Changes.Select(kvp => kvp.Key);
            var followUpEvents = await UpdateTilesAsync(affectedTiles, transaction);
            return new ResetResult(transaction, true, followUpEvents);
        }

        private async Task<bool> MoveCoreAsync(Point sourcePosition, Point targetPosition, ITransactionWithMoveSupport transaction)
        {
            var source = await GetAsync(sourcePosition);
            var target = await GetAsync(targetPosition);

            var oldSource = source;
            var oldTarget = target;

            source.Tile.EnsureNotNull();
            target.Tile.EnsureNotNull();

            if (source.Tile.Entity == Entity.None)
            {
                // Cancel if there's no entity to move
                transaction.IsSealed = true;
                return false;
            }

            var move = new EntityMoveInfo(source.Tile.Entity, sourcePosition, targetPosition);
            transaction.Moves.Push(move);

            IBeginMoveArgs beginMoveArgs = new GameplayArgs(transaction, this);
            IDetachArgs detachArgs = new GameplayArgs(transaction, this);
            IAttachArgs attachArgs = new GameplayArgs(transaction, this);

            // OnBeginMove: Notify entity that a move has been initiated
            await source.Tile.Entity.BeginMoveAsync(beginMoveArgs);
            beginMoveArgs.ValidateResult();
            if (transaction.IsSealed) return false;

            var newSource = source.WithTile(Tile.Compose(source.Tile, beginMoveArgs.ResultingEntity));
            if (transaction.Set(sourcePosition, source, newSource))
                source = newSource;

            move.Entity = beginMoveArgs.ResultingEntity;

            // Detach: Remove the entity from the source tile
            await source.Tile.DetachEntityAsync(detachArgs);
            detachArgs.ValidateResult();
            if (transaction.IsSealed) return false;

            // Attach: Add the entity to the target tile
            await target.Tile.AttachEntityAsync(attachArgs);
            attachArgs.ValidateResult();
            if (transaction.IsSealed) return false;

            // Detach from old tile, attach to new tile have succeeded
            // => Apply changes to transaction
            var newSource2 = source.WithTile(detachArgs.Result);
            if (transaction.Set(sourcePosition, source, newSource2))
                source = newSource2;

            var newTarget = target.WithTile(attachArgs.Result);
            if (transaction.Set(targetPosition, target, newTarget))
                target = newTarget;

            // Emit events
            var moveEvent = new EntityMoveEvent(sourcePosition, targetPosition, oldSource.Tile, target.Tile);
            transaction.Emit(moveEvent);

            if (source.Tile.Entity != Entity.None)
            {
                // A new entity has been created at source position
                var spawnEvent = new EntitySpawnEvent(sourcePosition, source.Tile.Entity);
                transaction.Emit(spawnEvent);
            }

            if (oldTarget.Tile.Entity != Entity.None)
            {
                // The entity at target position has either been replaced
                // or it has been moved/collected/... during a nested move
                var oldTargetEntityOverwritten =
                    !(transaction.Events.OfType<EntityDespawnEvent>().Any(ev => ev.Position == targetPosition) ||
                    transaction.Events.OfType<EntityMoveEvent>().Any(ev => ev.SourcePosition == targetPosition));

                if (oldTargetEntityOverwritten)
                {
                    // If replaced, emit despawn event
                    var despawnEvent = new EntityDespawnEvent(targetPosition, oldTarget.Tile.Entity);
                    transaction.Emit(despawnEvent);
                }
            }

            transaction.Moves.Pop();
            return true;
        }

        private async void OnChunkLoaded(KeyValuePair<Point, IChunk> kvp)
        {
            for (var y = 0; y < Chunk.Size; y++)
            {
                for (var x = 0; x < Chunk.Size; x++)
                {
                    var localPosition = new Point(x, y);
                    var globalPosition = kvp.Key * Chunk.Size + localPosition;
                    var tileInfo = kvp.Value[localPosition];
                    Dependencies.AddDependenciesOf(tileInfo.Tile, globalPosition);

                    // For entities emit a spawn event
                    if (tileInfo.Tile.Entity != Entity.None)
                    {
                        var spawnEvent = new EntitySpawnEvent(globalPosition, tileInfo.Tile.Entity);
                        _eventSource.OnNext(spawnEvent);
                    }
                }
            }

            // Properly initialize the chunk.
            // Example scenarios: On chunk loading...
            // - a button should be activated immediately if there's an entity pressing it
            // - a balloon should be popped immediately if it is on a pin
            var chunkPoints = new Rectangle(kvp.Key.X * Chunk.Size, kvp.Key.Y * Chunk.Size, Chunk.Size - 1, Chunk.Size - 1);
            var transaction = new TransactionWithMoveSupport(MoveInitiator.Empty);
            await UpdateTilesAsync(chunkPoints, transaction);
            _eventSource.OnNext(new ChunkAddedEvent(kvp.Value));

            // TODO: This is problematic! A commit is done directly on the map
            // bypassing the GameServer. We might have to forward the new chunk
            // to some of the players.
            // On client side we can't just invent some new version numbers.
            await transaction.CommitAsync(this, Metadata?.NextTileVersion() ?? 0);
        }

        private void OnChunkUnloaded(KeyValuePair<Point, IChunk> kvp)
        {
            for (var y = 0; y < Chunk.Size; y++)
            {
                for (var x = 0; x < Chunk.Size; x++)
                {
                    var localPosition = new Point(x, y);
                    var globalPosition = kvp.Key * Chunk.Size + localPosition;
                    var tileInfo = kvp.Value[localPosition];
                    Dependencies.RemoveDependenciesOf(tileInfo.Tile, globalPosition);
                    Dependencies.RemoveDependenciesOn(globalPosition);

                    // For entities emit a despawn event
                    if (tileInfo.Tile.Entity != Entity.None)
                    {
                        var despawnEvent = new EntityDespawnEvent(globalPosition, tileInfo.Tile.Entity);
                        _eventSource.OnNext(despawnEvent);
                    }
                }
            }

            _eventSource.OnNext(new ChunkRemovedEvent(kvp.Value));
        }

        public async Task<IReadOnlyCollection<FollowUpEvent>> UpdateTilesAsync(IEnumerable<Point> positions, ITransactionWithMoveSupport transaction)
        {
            var initialPoints = new HashSet<Point>(positions);
            var followUpEvents = new List<FollowUpEvent>();
            var now = DateTimeOffset.Now;

            // Notify the initial tiles and tiles that are directly or indirectly
            // (transitively) dependent on them in topological order
            await Dependencies.DoAsyncWorkFollowingDependenciesAsync(initialPoints, async p =>
            {
                // Try to get from transaction first; if that fails get from map
                var tileInfo = transaction.Changes.TryGetValue(p);
                if (tileInfo == TileInfo.Empty) tileInfo = await GetAsync(p);

                followUpEvents.Add(new FollowUpEvent(p, transaction.Initiator, now + tileInfo.Tile.FollowUpDelay));

                var completionArgs = new GameplayArgs(transaction, this);
                await tileInfo.Tile.OnEntityMoveTransactionCompletedAsync(completionArgs);
                completionArgs.ValidateResult();

                if (transaction.IsSealed)
                    return null; // Null terminates the loop

                var newTileInfo = tileInfo.WithTile(completionArgs.Result);
                
                if (transaction.Set(p, tileInfo, newTileInfo))
                {
                    // TODO: Test and check whether this is sufficient
                    // (Initial idea:
                    // Somehow modify events that have been emitted during the move.
                    // E.g. when a balloon is moved onto an InkTile the move event's target
                    // still contains the balloon with its old color.
                    // Here, after the InkTile has changed the balloons color, we have to
                    // change that event to reflect the changes of InkTile and balloon.
                    // We probably have to emit spawn/despawn events as well.)
                    if (tileInfo.Tile.Entity != Entity.None)
                    {
                        if (newTileInfo.Tile.Entity == Entity.None)
                            transaction.Emit(new EntityDespawnEvent(p, tileInfo.Tile.Entity));
                        else
                            transaction.Emit(new EntityChangeEvent(p, newTileInfo.Tile.Entity));
                    }
                    else
                    {
                        if (newTileInfo.Tile.Entity != Entity.None)
                            transaction.Emit(new EntitySpawnEvent(p, newTileInfo.Tile.Entity));
                    }
                    return true;
                }

                return initialPoints.Contains(p);
            });

            return followUpEvents;
        }
    }
}
