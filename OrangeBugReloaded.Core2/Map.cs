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

        public ChunkLoader ChunkLoader { get; }

        /// <summary>
        /// Stores the dependencies between tiles on the map.
        /// </summary>
        public MapDependencyTable Dependencies { get; } = new MapDependencyTable();

        /// <inheritdoc/>
        public IObservable<IGameEvent> Events => _eventSource;

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
        }

        /// <inheritdoc/>
        public async Task<Tile> GetAsync(Point position, MapLayer layer = MapLayer.Gameplay)
        {
            var chunk = await ChunkLoader.TryGetAsync(position / Chunk.Size);
            return chunk[position % Chunk.Size, layer];
        }

        /// <inheritdoc/>
        public async Task<bool> SetAsync(Point position, Tile tile, MapLayer layer = MapLayer.Gameplay)
        {
            // TODO: Handle concurrency
            var chunk = await ChunkLoader.TryGetAsync(position / Chunk.Size);
            var oldTile = chunk[position % Chunk.Size, layer];

            if (!Equals(oldTile, tile))
            {
                // Update chunk
                chunk[position % Chunk.Size, layer] = tile;

                // Update dependencies
                Dependencies.RemoveDependenciesOf(oldTile, position);
                Dependencies.AddDependenciesOf(tile, position);

                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> MoveAsync(Point sourcePosition, Point targetPosition)
        {
            var rootTransaction = new EntityMoveTransaction(this);
            var isSuccessful = await MoveAsync(sourcePosition, targetPosition, rootTransaction);

            if (!isSuccessful)
                return false;

            // Commit - apply changes of all transactions to map beginning with the
            // oldest transaction so that newer ones can overwrite changes of older ones.
            var currentTransaction = rootTransaction;

            while (currentTransaction != null)
            {
                foreach (var kvp in currentTransaction.ChangedTiles)
                    await SetAsync(kvp.Key, kvp.Value);

                currentTransaction.FlushEvents(_eventSource);
                currentTransaction = (EntityMoveTransaction)currentTransaction.Next;
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> MoveAsync(Point sourcePosition, Point targetPosition, EntityMoveTransaction transaction)
        {
            // Execute the actual move.
            // Any changes to the map are "recorded" in the transaction
            // and can later be applied to the map via a commit.
            await MoveCoreAsync(sourcePosition, targetPosition, transaction);

            if (transaction.IsCancelled)
                return false;

            // At the end of the recursion...
            if (transaction.Moves.Count == 0)
            {
                var notifiedPositions = new HashSet<Point>();

                // Tiles that definitely must be notified are the ones that changed
                // and the ones that depend on any of the changed tiles.
                var affectedTiles = transaction.ChangedTiles.Select(kvp => kvp.Key)
                    .Concat(transaction.ChangedTiles.SelectMany(p => Dependencies.GetDependenciesOn(p.Key)));

                // Notify affected tiles and dependent tiles in topological order
                await Dependencies.DoAsyncWorkFollowingDependenciesAsync(affectedTiles, async p =>
                {
                    notifiedPositions.Add(p);
                    var tile = await transaction.Last.GetAsync(p);
                    var completionArgs = new TileEventArgs(transaction);
                    await tile.OnEntityMoveTransactionCompletedAsync(completionArgs);
                    completionArgs.ValidateResult();

                    if (transaction.IsCancelled)
                        return null; // Null terminates the loop

                    return await transaction.SetAsync(p, completionArgs.Result);
                });

                // Trigger follow-up transactions
                // e.g. a teleporter might initiate a new transaction here to teleport an entity
                var args = new FollowUpEventArgs(transaction);

                foreach (var p in notifiedPositions)
                {
                    var tile = await transaction.Last.GetAsync(p);
                    await tile.OnFollowUpTransactionAsync(args, p);
                }
            }

            return true;
        }

        private async Task MoveCoreAsync(Point sourcePosition, Point targetPosition, EntityMoveTransaction transaction)
        {
            var move = new EntityMoveInfo
            {
                SourcePosition = sourcePosition,
                TargetPosition = targetPosition
            };

            transaction.Moves.Push(move);

            var source = await transaction.GetAsync(sourcePosition);
            var target = await transaction.GetAsync(targetPosition);
            move.Entity = source.Entity;

            // OnBeginMove: Notify entity that a move has been initiated
            var beginMoveArgs = new EntityEventArgs(transaction);
            await source.Entity.BeginMoveAsync(beginMoveArgs);
            beginMoveArgs.ValidateResult();
            if (transaction.IsCancelled) return;
            source = Tile.Compose(source, beginMoveArgs.Result);
            await transaction.SetAsync(sourcePosition, source);
            move.Entity = beginMoveArgs.Result;

            // Detach: Remove the entity from the source tile
            var detachArgs = new TileEventArgs(transaction);
            await source.DetachEntityAsync(detachArgs);
            detachArgs.ValidateResult();
            if (transaction.IsCancelled) return;
            source = detachArgs.Result;
            await transaction.SetAsync(sourcePosition, source);

            // Attach: Add the entity to the target tile
            var attachArgs = new TileEventArgs(transaction);
            await target.AttachEntityAsync(attachArgs);
            attachArgs.ValidateResult();
            if (transaction.IsCancelled) return;
            target = attachArgs.Result;
            await transaction.SetAsync(targetPosition, target);

            transaction.Moves.Pop();
        }

        /// <summary>
        /// Applies changes collected in transactions to the map.
        /// </summary>
        /// <param name="transaction">
        /// A transaction. This can be any transaction within a chain of transactions;
        /// the method automatically starts with the first transaction in the chain.
        /// </param>
        private async Task CommitTransactionAsync(IReadOnlyMapTransaction transaction)
        {
            // Commit - apply changes of all transactions to map beginning with the
            // oldest transaction so that newer ones can overwrite changes of older ones.
            var currentTransaction = transaction.First;

            while (currentTransaction != null)
            {
                foreach (var kvp in currentTransaction.ChangedTiles)
                    await SetAsync(kvp.Key, kvp.Value);

                currentTransaction = (EntityMoveTransaction)currentTransaction.Next;
            }
        }

        private void OnChunkUnloaded(KeyValuePair<Point, IChunk> kvp)
        {
            for (var y = 0; y < Chunk.Size; y++)
            {
                for (var x = 0; x < Chunk.Size; x++)
                {
                    var p = new Point(x, y);
                    Dependencies.RemoveDependenciesOf(kvp.Value[p, MapLayer.Gameplay], p);
                    Dependencies.RemoveDependenciesOn(p);
                }
            }

            _eventSource.OnNext(new ChunkRemovedEvent(kvp.Value));
        }

        private void OnChunkLoaded(KeyValuePair<Point, IChunk> kvp)
        {
            for (var y = 0; y < Chunk.Size; y++)
            {
                for (var x = 0; x < Chunk.Size; x++)
                {
                    var p = new Point(x, y);
                    Dependencies.AddDependenciesOf(kvp.Value[p, MapLayer.Gameplay], p);
                }
            }

            _eventSource.OnNext(new ChunkAddedEvent(kvp.Value));
        }
    }
}
