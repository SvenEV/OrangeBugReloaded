using OrangeBugReloaded.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Transactions
{
    public class TransactionChain : ITransactionChain
    {
        private readonly Func<ITransaction> _transactionFactory;
        private readonly List<ITransaction> _transactions = new List<ITransaction>();
        private IMap _map;

        public ITransaction CurrentTransaction => _transactions.Last();

        public ITransaction RootTransaction => _transactions.First();

        protected TransactionChain(IMap map, Func<ITransaction> transactionFactory)
        {
            _transactionFactory = transactionFactory;
            _transactions = new List<ITransaction> { transactionFactory() };
            _map = map;
        }

        /// <summary>
        /// Initializes a new <see cref="TransactionChain"/> of
        /// the specified transaction type.
        /// </summary>
        /// <typeparam name="T">
        /// The transaction type. Must have a public constructor without parameters
        /// so that the transaction chain can instantiate new transactions.
        /// </typeparam>
        /// <param name="map">The underlying map</param>
        /// <returns>The transaction chain</returns>
        public static TransactionChain Create<T>(IGameplayMap map) where T : ITransaction, new()
        {
            return new TransactionChain(map, () => new T());
        }

        /// <inheritdoc/>
        public void Emit(IGameEvent e)
        {
            CurrentTransaction.Events.Add(e);
        }

        /// <inheritdoc/>
        public ITransaction CreateFollowUpTransaction()
        {
            var newTransaction = _transactionFactory();
            _transactions.Add(newTransaction);
            return newTransaction;
        }

        /// <inheritdoc/>
        public async Task CommitAsync(IObserver<IGameEvent> eventSource)
        {
            // Apply changes of all transactions to the map beginning with the
            // oldest transaction so that newer ones can overwrite changes of older ones.
            foreach (var currentTransaction in _transactions)
            {
                // Apply recorded changes to map
                foreach (var kvp in currentTransaction.ChangedTiles)
                    await _map.SetAsync(kvp.Key, kvp.Value);

                // Flush recorded events
                foreach (var e in currentTransaction.Events)
                    eventSource.OnNext(e);
            }
        }

        /// <inheritdoc/>
        public async Task<Tile> GetAsync(Point position, MapLayer layer = MapLayer.Gameplay)
        {
            // TODO: In the future we would like to use transactions for designing purposes as well
            if (layer != MapLayer.Gameplay)
                throw new ArgumentException("Not supported: layer must be Gameplay", nameof(layer));

            // Try to get the tile from the current transaction.
            // If current transaction doesn't have it, try to get from
            // the transaction before and so on.
            var tile = _transactions
                .Select(t => t.ChangedTiles.TryGetValue(position))
                .LastOrDefault(t => t != null);

            // If no transaction has a changed tile at that position,
            // get it directly from the underlying map.
            if (tile == null)
            {
                // Otherwise, load tile from parent transaction or,
                // if this is already the first transaction, directly from the map.
                tile = await _map.GetAsync(position, layer);
            }

            return tile;
        }

        /// <inheritdoc/>
        public async Task<bool> SetAsync(Point position, Tile tile, MapLayer layer = MapLayer.Gameplay)
        {
            if (CurrentTransaction.IsCancelled)
                return false;

            var currentTile = await GetAsync(position, layer);

            if (!Equals(currentTile, tile))
            {
                // If tile actually differs from the one in the transactions
                // or on the map, add it to the list of changed tiles.
                CurrentTransaction.ChangedTiles[position] = tile;
                return true;
            }

            return false;
        }
    }
}
