using OrangeBugReloaded.Core.Events;
using System;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Transactions
{
    public class TransactionChainWithMoveSupport : TransactionChainBase<Tile>, ITransactionChainWithMoveSupport, IMap
    {
        private IGameplayMap _map;

        /// <inheritdoc/>
        public object Initiator { get; set; }

        /// <inheritdoc/>
        ITransactionWithMoveSupport ITransactionChainWithMoveSupport.CurrentTransaction => (ITransactionWithMoveSupport)CurrentTransaction;

        private TransactionChainWithMoveSupport(IGameplayMap map, Func<ITransactionWithMoveSupport> transactionFactory)
            : base(map, transactionFactory)
        {
            _map = map;
        }
        
        /// <summary>
        /// Initializes a new <see cref="TransactionChainWithMoveSupport"/> of
        /// the specified transaction type.
        /// </summary>
        /// <typeparam name="T">
        /// The transaction type. Must have a public constructor without parameters
        /// so that the transaction chain can instantiate new transactions.
        /// </typeparam>
        /// <param name="map">The underlying map</param>
        /// <returns>The transaction chain</returns>
        public static TransactionChainWithMoveSupport Create<T>(IGameplayMap map) where T : ITransactionWithMoveSupport, new()
        {
            return new TransactionChainWithMoveSupport(map, () => new T());
        }

        /// <inheritdoc/>
        public async Task<bool> MoveAsync(Point sourcePosition, Point targetPosition)
        {
            if (CurrentTransaction.IsCanceled)
                return false;

            return await _map.MoveAsync(sourcePosition, targetPosition, this);
        }

        /// <inheritdoc/>
        public override async Task CommitAsync(IObserver<IGameEvent> eventSource)
        {
            // Apply changes of all transactions to the map beginning with the
            // oldest transaction so that newer ones can overwrite changes of older ones.
            foreach (var currentTransaction in Transactions)
            {
                // Apply recorded changes to map
                foreach (var kvp in currentTransaction.Changes)
                    await _map.SetAsync(kvp.Key, kvp.Value);

                // Flush recorded events
                foreach (var e in currentTransaction.Events)
                    eventSource?.OnNext(e);
            }
        }

        /// <inheritdoc/>
        public async Task<Tile> GetAsync(Point position)
        {
            // Get the latest recorded change if one is available,
            // otherwise get the tile directly from the underlying map
            return GetLatest(position) ?? await _map.GetAsync(position);
        }

        /// <inheritdoc/>
        public async Task<bool> SetAsync(Point position, Tile tile)
        {
            if (CurrentTransaction.IsCanceled)
                return false;

            var currentTile = await GetAsync(position);

            if (!Equals(currentTile, tile))
            {
                // If tile actually differs from the one in the transactions
                // or on the map, add it to the list of changed tiles.
                Set(position, tile);
                return true;
            }

            return false;
        }

        Task<TileMetadata> IReadOnlyMap.GetMetadataAsync(Point position)
        {
            throw new NotSupportedException();
        }

        Task<bool> IMap.SetMetadataAsync(Point position, TileMetadata value)
        {
            throw new NotSupportedException();
        }
    }
}
