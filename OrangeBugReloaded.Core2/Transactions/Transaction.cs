using OrangeBugReloaded.Core.Events;
using System.Collections.Generic;

namespace OrangeBugReloaded.Core.Transactions
{
    /// <summary>
    /// Implements the base functionality of a transaction.
    /// </summary>
    public abstract class TransactionBase<T> : ITransaction<T>
    {
        /// <inheritdoc/>
        public IDictionary<Point, T> Changes { get; } = new Dictionary<Point, T>();

        /// <inheritdoc/>
        public IList<IGameEvent> Events { get; } = new List<IGameEvent>();
        
        /// <inheritdoc/>
        public bool IsCanceled { get; private set; }
        
        /// <inheritdoc/>
        public void Cancel()
        {
            IsCanceled = true;
            Changes.Clear();
        }
    }
}
