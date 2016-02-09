using OrangeBugReloaded.Core.Events;
using System;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Transactions
{
    public interface ITransactionChain<T> : IGameEventEmitter
    {
        /// <summary>
        /// Gets the latest transaction in the chain.
        /// </summary>
        ITransaction<T> CurrentTransaction { get; }

        /// <summary>
        /// Applies changes collected in transactions to the map.
        /// </summary>
        /// <param name="transaction">
        /// A transaction. This can be any transaction within a chain of transactions;
        /// the method automatically starts with the first transaction in the chain.
        /// </param>
        Task CommitAsync(IObserver<IGameEvent> eventSource);

        /// <summary>
        /// Adds a transaction to the transaction chain.
        /// </summary>
        /// <returns>The new transaction</returns>
        ITransaction<T> CreateFollowUpTransaction();
    }
}
