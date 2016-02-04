using System;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Transactions
{
    public class TransactionChainWithMoveSupport : TransactionChain, ITransactionChainWithMoveSupport
    {
        private IGameplayMap _map;

        /// <inheritdoc/>
        public object Initiator { get; set; }

        /// <inheritdoc/>
        ITransactionWithMoveSupport ITransactionChainWithMoveSupport.CurrentTransaction => (ITransactionWithMoveSupport)CurrentTransaction;

        /// <inheritdoc/>
        ITransactionWithMoveSupport ITransactionChainWithMoveSupport.RootTransaction => (ITransactionWithMoveSupport)RootTransaction;

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
        public static new TransactionChainWithMoveSupport Create<T>(IGameplayMap map) where T : ITransactionWithMoveSupport, new()
        {
            return new TransactionChainWithMoveSupport(map, () => new T());
        }

        /// <inheritdoc/>
        public async Task<bool> MoveAsync(Point sourcePosition, Point targetPosition)
        {
            if (CurrentTransaction.IsCancelled)
                return false;

            return await _map.MoveAsync(sourcePosition, targetPosition, this);
        }
    }
}
