using System;
using System.Threading.Tasks;
using OrangeBugReloaded.Core.Events;

namespace OrangeBugReloaded.Core.Transactions
{
    public interface ITransactionChainWithEditSupport : ITransactionChain<TileMetadata>, IMap
    {
    }

    public class TransactionChainWithEditSupport : TransactionChainBase<TileMetadata>, ITransactionChainWithEditSupport
    {
        private IMap _map;

        private TransactionChainWithEditSupport(IMap map, Func<ITransaction<TileMetadata>> transactionFactory)
            : base(map, transactionFactory)
        {
            _map = map;
        }

        public static TransactionChainWithEditSupport Create<T>(IGameplayMap map) where T : ITransaction<TileMetadata>, new()
        {
            return new TransactionChainWithEditSupport(map, () => new T());
        }

        public override async Task CommitAsync(IObserver<IGameEvent> eventSource)
        {
            // Apply changes of all transactions to the map beginning with the
            // oldest transaction so that newer ones can overwrite changes of older ones.
            foreach (var currentTransaction in Transactions)
            {
                // Apply recorded changes to map
                foreach (var kvp in currentTransaction.Changes)
                    await _map.SetMetadataAsync(kvp.Key, kvp.Value);

                // Flush recorded events
                foreach (var e in currentTransaction.Events)
                    eventSource?.OnNext(e);
            }
        }


        public async Task<TileMetadata> GetMetadataAsync(Point position)
        {
            var meta = GetLatest(position);
            return (meta != TileMetadata.Empty) ? meta : await _map.GetMetadataAsync(position);
        }

        public async Task<bool> SetMetadataAsync(Point position, TileMetadata value)
        {
            if (CurrentTransaction.IsCanceled)
                return false;

            var currentMeta = await GetMetadataAsync(position);

            if (!Equals(currentMeta, value))
            {
                // If the metadata actually differs from the one in the transactions
                // or on the map, add it to the list of changes
                Set(position, value);
                return true;
            }

            return false;
        }

        Task<Tile> IReadOnlyMap.GetAsync(Point position)
        {
            throw new NotSupportedException();
        }

        Task<bool> IMap.SetAsync(Point position, Tile tile)
        {
            throw new NotSupportedException();
        }
    }
}
