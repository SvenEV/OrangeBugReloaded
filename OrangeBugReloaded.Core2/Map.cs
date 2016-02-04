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
            var transactionChain = TransactionChainWithMoveSupport.Create<TransactionWithMoveSupport>(this);

            var isSuccessful = await MoveAsync(sourcePosition, targetPosition, transactionChain);

            if (isSuccessful)
            {
                await transactionChain.CommitAsync(_eventSource);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> MoveAsync(Point sourcePosition, Point targetPosition, ITransactionChainWithMoveSupport transactionChain)
        {
            // Execute the actual move.
            // Any changes to the map are "recorded" in the transaction
            // and can later be applied to the map via a commit.
            await MoveCoreAsync(sourcePosition, targetPosition, transactionChain);

            var transaction = transactionChain.CurrentTransaction;

            if (transaction.IsCancelled)
                return false;

            // At the end of the recursion...
            if (transaction.Moves.Count == 0)
            {
                // Notify changed tiles and tiles that directly or indirectly depend on changed tiles
                var affectedTiles = transaction.ChangedTiles.Select(kvp => kvp.Key);
                await UpdateTilesAsync(affectedTiles, transactionChain);
            }

            return true;
        }

        private async Task MoveCoreAsync(Point sourcePosition, Point targetPosition, ITransactionChainWithMoveSupport transactionChain)
        {
            var source = await transactionChain.GetAsync(sourcePosition);
            var target = await transactionChain.GetAsync(targetPosition);
            var originalEntity = source.Entity;

            var move = new EntityMoveInfo
            {
                SourcePosition = sourcePosition,
                TargetPosition = targetPosition,
                Entity = source.Entity
            };

            var transaction = transactionChain.CurrentTransaction;

            transaction.Moves.Push(move);

            source.Entity.EnsureNotNone();

            // OnBeginMove: Notify entity that a move has been initiated
            var beginMoveArgs = new EntityEventArgs(transactionChain);
            await source.Entity.BeginMoveAsync(beginMoveArgs);
            beginMoveArgs.ValidateResult();
            if (transaction.IsCancelled) return;
            source = Tile.Compose(source, beginMoveArgs.Result);
            await transactionChain.SetAsync(sourcePosition, source);
            move.Entity = beginMoveArgs.Result;

            // Detach: Remove the entity from the source tile
            var detachArgs = new DetachEventArgs(transactionChain);
            await source.DetachEntityAsync(detachArgs);
            detachArgs.ValidateResult();
            if (transaction.IsCancelled) return;

            // Attach: Add the entity to the target tile
            var attachArgs = new AttachEventArgs(transactionChain);
            await target.AttachEntityAsync(attachArgs);
            attachArgs.ValidateResult();
            if (transaction.IsCancelled) return;

            if (!attachArgs.PreventDetach)
            {
                source = detachArgs.Result;
                await transactionChain.SetAsync(sourcePosition, detachArgs.Result);
            }

            if (!detachArgs.PreventAttach)
            {
                target = attachArgs.Result;
                await transactionChain.SetAsync(targetPosition, attachArgs.Result);
            }

            transactionChain.Emit(new EntityMoveEvent(sourcePosition, targetPosition, originalEntity, target.Entity));

            transaction.Moves.Pop();
        }

        private void OnChunkUnloaded(KeyValuePair<Point, IChunk> kvp)
        {
            for (var y = 0; y < Chunk.Size; y++)
            {
                for (var x = 0; x < Chunk.Size; x++)
                {
                    var localPosition = new Point(x, y);
                    var globalPosition = kvp.Value.Index * Chunk.Size + localPosition;
                    Dependencies.RemoveDependenciesOf(kvp.Value[localPosition, MapLayer.Gameplay], globalPosition);
                    Dependencies.RemoveDependenciesOn(globalPosition);
                }
            }

            _eventSource.OnNext(new ChunkRemovedEvent(kvp.Value));
        }

        private async void OnChunkLoaded(KeyValuePair<Point, IChunk> kvp)
        {
            for (var y = 0; y < Chunk.Size; y++)
            {
                for (var x = 0; x < Chunk.Size; x++)
                {
                    var localPosition = new Point(x, y);
                    var globalPosition = kvp.Value.Index * Chunk.Size + localPosition;
                    Dependencies.AddDependenciesOf(kvp.Value[localPosition, MapLayer.Gameplay], globalPosition);
                }
            }

            // Properly initialize the chunk.
            // Example scenarios: On chunk loading...
            // - a button should be activated immediately if there's an entity pressing it
            // - a balloon should be popped immediately if it is on a pin
            var chunkPoints = new Rectangle(kvp.Value.Index.X * Chunk.Size, kvp.Value.Index.Y * Chunk.Size, Chunk.Size - 1, Chunk.Size - 1);
            var transactionChain = TransactionChainWithMoveSupport.Create<TransactionWithMoveSupport>(this);
            await UpdateTilesAsync(chunkPoints, transactionChain);
            _eventSource.OnNext(new ChunkAddedEvent(kvp.Value));
            await transactionChain.CommitAsync(_eventSource);
        }

        private async Task UpdateTilesAsync(IEnumerable<Point> positions, ITransactionChainWithMoveSupport transactionChain)
        {
            var initialPoints = new HashSet<Point>(positions);
            var notifiedPositions = new HashSet<Point>();

            // Notify the initial tiles and tiles that are directly or indirectly
            // (transitively) dependent on them in topological order
            await Dependencies.DoAsyncWorkFollowingDependenciesAsync(initialPoints, async p =>
            {
                notifiedPositions.Add(p);

                var tile = await transactionChain.GetAsync(p);

                var completionArgs = new TileEventArgs(transactionChain);
                await tile.OnEntityMoveTransactionCompletedAsync(completionArgs);
                completionArgs.ValidateResult();

                if (transactionChain.CurrentTransaction.IsCancelled)
                    return null; // Null terminates the loop

                var hasChanged = await transactionChain.SetAsync(p, completionArgs.Result);
                return hasChanged || initialPoints.Contains(p);
            });

            // Trigger follow-up transactions
            // e.g. a teleporter might initiate a new transaction here to teleport an entity
            foreach (var p in notifiedPositions)
            {
                var args = new FollowUpEventArgs(transactionChain);
                var tile = await transactionChain.GetAsync(p);
                await tile.OnFollowUpTransactionAsync(args, p);
            }
        }
    }
}
