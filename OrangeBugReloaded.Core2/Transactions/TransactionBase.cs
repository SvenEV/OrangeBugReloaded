using OrangeBugReloaded.Core.Events;
using System.Collections.Generic;

namespace OrangeBugReloaded.Core.Transactions
{
    /// <summary>
    /// Implements the base functionality of a transaction.
    /// </summary>
    public abstract class TransactionBase<T> : ITransaction<T>
    {
        private Dictionary<Point, T> _changes = new Dictionary<Point, T>();
        private List<IGameEvent> _events = new List<IGameEvent>();

        /// <inheritdoc/>
        public IReadOnlyDictionary<Point, T> Changes => _changes;

        /// <inheritdoc/>
        public IReadOnlyList<IGameEvent> Events => _events;

        /// <inheritdoc/>
        public MoveInitiator Initiator { get; set; }
        
        /// <inheritdoc/>
        public bool IsSealed { get; set; }

        public TransactionBase(MoveInitiator initiator)
        {
            Initiator = initiator;
        }

        /// <inheritdoc/>
        public bool Set(Point position, T oldValue, T value)
        {
            if (IsSealed || Equals(oldValue, value))
                return false;

            _changes[position] = value;
            return true;
        }
        
        /// <inheritdoc/>
        public void Emit(IGameEvent e)
        {
            if (IsSealed)
                return;

            _events.Add(e);
        }
    }
}
