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

            // TODO: For now, just create metadata instead of loading from storage
            Metadata = new MapMetadata();
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
        [Obsolete("Use MoveAsync(Point, Point, ITransactionWithMoveSupport) instead", true)]
        public async Task<MoveResult> MoveAsync(Point sourcePosition, Point targetPosition)
        {
            throw new NotImplementedException();
            //var transactionChain = TransactionChainWithMoveSupport.Create<TransactionWithMoveSupport>(this);

            //var moveResult = await MoveAsync(sourcePosition, targetPosition, transactionChain);

            // TODO: Move the responsibility of committing transactions outside the map
            // Clients should only use (and eventually commit) the first transaction in the chain
            // Follow-up transactions should be triggered by the server, changes are pushed to clients.
            //if (moveResult.IsSuccessful)
            //{
            //    await transactionChain.CommitAsync(_eventSource);
            //    return new MoveResult(transactionChain);
            //}
            //else
            //{
            //    return new MoveResult(transactionChain);
            //}
        }

        /// <inheritdoc/>
        public async Task<MoveResult> MoveAsync(Point sourcePosition, Point targetPosition, ITransactionWithMoveSupport transaction)
        {
            // Execute the actual move.
            // Any changes to the map are "recorded" in the transaction
            // and can later be applied to the map via a commit.
            await MoveCoreAsync(sourcePosition, targetPosition, transaction);

            // At the end of the recursion...
            if (transaction.Moves.Count == 0 && !transaction.IsCanceled)
            {
                // Notify changed tiles and tiles that directly or indirectly depend on changed tiles
                var affectedTiles = transaction.Changes.Select(kvp => kvp.Key);
                var followUpEvents = await UpdateTilesAsync(affectedTiles, transaction);
                return new MoveResult(transaction, followUpEvents);
            }

            return new MoveResult(transaction, Enumerable.Empty<FollowUpEvent>());
        }

        /// <inheritdoc/>
        public async Task<MoveResult> SpawnAsync(Entity entity, Point position)
        {
            var tileInfo = await GetAsync(position);

            tileInfo.Tile.EnsureNotNull();

            var transaction = new TransactionWithMoveSupport(MoveInitiator.Empty);
            transaction.Moves.Push(new EntityMoveInfo
            {
                Entity = entity,
                SourcePosition = position,
                TargetPosition = position
            });

            // Try to attach entity
            var attachArgs = new AttachEventArgs(transaction, this);
            await tileInfo.Tile.AttachEntityAsync(attachArgs);
            attachArgs.ValidateResult();

            if (attachArgs.IsCanceled)
                return new MoveResult(transaction, Enumerable.Empty<FollowUpEvent>());

            var newTileInfo = tileInfo.WithTile(attachArgs.Result);
            transaction.Changes[position] = newTileInfo;

            // Update tile (note that this cannot cancel the transaction)
            var followUpEvents = await UpdateTilesAsync(new[] { position }, transaction);

            return new MoveResult(transaction, followUpEvents);
        }

        private async Task MoveCoreAsync(Point sourcePosition, Point targetPosition, ITransactionWithMoveSupport transaction)
        {
            var source = await GetAsync(sourcePosition);
            var target = await GetAsync(targetPosition);

            source.Tile.EnsureNotNull();
            target.Tile.EnsureNotNull();
            source.Tile.Entity.EnsureNotNone();

            var oldSource = source;

            var move = new EntityMoveInfo
            {
                SourcePosition = sourcePosition,
                TargetPosition = targetPosition,
                Entity = source.Tile.Entity
            };

            transaction.Moves.Push(move);

            // OnBeginMove: Notify entity that a move has been initiated
            var beginMoveArgs = new EntityEventArgs(transaction, this);
            await source.Tile.Entity.BeginMoveAsync(beginMoveArgs);
            beginMoveArgs.ValidateResult();
            if (transaction.IsCanceled) return;
            if (!Equals(source.Tile, beginMoveArgs.Result))
            {
                source = source.WithTile(Tile.Compose(source.Tile, beginMoveArgs.Result));
                transaction.Changes[sourcePosition] = source;
            }
            move.Entity = beginMoveArgs.Result;

            // Detach: Remove the entity from the source tile
            var detachArgs = new DetachEventArgs(transaction, this);
            await source.Tile.DetachEntityAsync(detachArgs);
            detachArgs.ValidateResult();
            if (transaction.IsCanceled) return;

            // Attach: Add the entity to the target tile
            var attachArgs = new AttachEventArgs(transaction, this);
            if (!detachArgs.PreventAttach)
            {
                await target.Tile.AttachEntityAsync(attachArgs);
                attachArgs.ValidateResult();
                if (transaction.IsCanceled) return;
            }

            if (!attachArgs.PreventDetach)
            {
                if (!Equals(source.Tile, detachArgs.Result))
                {
                    source = source.WithTile(detachArgs.Result);
                    transaction.Changes[sourcePosition] = source;
                }
            }

            if (!detachArgs.PreventAttach)
            {
                if (!Equals(target.Tile, attachArgs.Result))
                {
                    target = target.WithTile(attachArgs.Result);
                    transaction.Changes[targetPosition] = target;
                }
            }


            // TODO: This event is not correct if PreventAttach/PreventDetach are used
            //transaction.Emit(new EntityMoveEvent(sourcePosition, targetPosition, oldSource.Tile, target.Tile));

            transaction.Moves.Pop();
        }

        private async void OnChunkLoaded(KeyValuePair<Point, IChunk> kvp)
        {
            for (var y = 0; y < Chunk.Size; y++)
            {
                for (var x = 0; x < Chunk.Size; x++)
                {
                    var localPosition = new Point(x, y);
                    var globalPosition = kvp.Key * Chunk.Size + localPosition;
                    Dependencies.AddDependenciesOf(kvp.Value[localPosition].Tile, globalPosition);
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
            await transaction.CommitAsync(this, Metadata.NextVersion(), _eventSource);
        }

        private void OnChunkUnloaded(KeyValuePair<Point, IChunk> kvp)
        {
            for (var y = 0; y < Chunk.Size; y++)
            {
                for (var x = 0; x < Chunk.Size; x++)
                {
                    var localPosition = new Point(x, y);
                    var globalPosition = kvp.Key * Chunk.Size + localPosition;
                    Dependencies.RemoveDependenciesOf(kvp.Value[localPosition].Tile, globalPosition);
                    Dependencies.RemoveDependenciesOn(globalPosition);
                }
            }

            _eventSource.OnNext(new ChunkRemovedEvent(kvp.Value));
        }

        private async Task<IReadOnlyCollection<FollowUpEvent>> UpdateTilesAsync(IEnumerable<Point> positions, ITransactionWithMoveSupport transaction)
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

                var completionArgs = new TileEventArgs(transaction, this);
                await tileInfo.Tile.OnEntityMoveTransactionCompletedAsync(completionArgs);
                completionArgs.ValidateResult();

                if (transaction.IsCanceled)
                    return null; // Null terminates the loop

                var newTileInfo = tileInfo.WithTile(completionArgs.Result);

                if (!Equals(tileInfo.Tile, newTileInfo.Tile))
                {
                    transaction.Changes[p] = newTileInfo;
                    return true;
                }

                return initialPoints.Contains(p);
            });

            // Trigger follow-up transactions
            // e.g. a teleporter might initiate a new transaction here to teleport an entity
            //foreach (var p in notifiedPositions)
            //{
            //    var args = new FollowUpEventArgs(this, transaction);
            //    var tileInfo = await GetAsync(p);
            //    await tileInfo.Tile.OnFollowUpTransactionAsync(args, p);
            //    followUpEvents.AddRange(args.FollowUpMoves);
            //}

            return followUpEvents;
        }
    }
}
