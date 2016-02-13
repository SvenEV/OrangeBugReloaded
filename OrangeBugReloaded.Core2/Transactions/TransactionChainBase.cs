using OrangeBugReloaded.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Transactions
{
    public abstract class TransactionChainBase<T> : ITransactionChain<T>
    {
        private readonly Func<ITransaction<T>> _transactionFactory;
        private IMap _map;

        protected IList<ITransaction<T>> Transactions { get; }
        public ITransaction<T> CurrentTransaction => Transactions.Last();
        public ITransaction<T> FirstTransaction => Transactions.First();

        protected TransactionChainBase(IMap map, Func<ITransaction<T>> transactionFactory)
        {
            Transactions = new List<ITransaction<T>> { transactionFactory() };
            _transactionFactory = transactionFactory;
            _map = map;
        }

        /// <inheritdoc/>
        public void Emit(IGameEvent e)
        {
            CurrentTransaction.Events.Add(e);
        }

        /// <inheritdoc/>
        public ITransaction<T> CreateFollowUpTransaction()
        {
            var newTransaction = _transactionFactory();
            Transactions.Add(newTransaction);
            return newTransaction;
        }

        /// <summary>
        /// Gets the latest value at the specified position.
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>The value (or the default value of <typeparamref name="T"/> if not found)</returns>
        protected T GetLatest(Point position)
        {
            T result;

            foreach (var t in Transactions.Reverse())
            {
                if (t.Changes.TryGetValue(position, out result))
                    return result;
            }

            return default(T);
        }

        /// <summary>
        /// Assigns the value to the specified position in the
        /// current transaction.
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="value">Value</param>
        protected void Set(Point position, T value)
        {
            CurrentTransaction.Changes[position] = value;
        }

        public abstract Task CommitAsync(IObserver<IGameEvent> eventSource);
    }
}
