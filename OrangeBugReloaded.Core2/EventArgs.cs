using OrangeBugReloaded.Core.Transactions;
using System;

namespace OrangeBugReloaded.Core
{
    public abstract class GameEventArgs<T>
    {
        public IMapTransactionWithMoveSupport Transaction { get; }

        public T Result { get; set; }

        public GameEventArgs(IMapTransactionWithMoveSupport transaction)
        {
            Transaction = transaction;
        }

        internal void ValidateResult()
        {
            if (Result == null && !Transaction.IsCancelled)
                throw new InvalidOperationException("Invalid result: No result is provided and the transaction is not cancelled");

            if (Result != null && Transaction.IsCancelled)
                throw new InvalidOperationException("Invalid result: The transaction is cancelled but a result is provided");
        }
    }

    public class EntityEventArgs : GameEventArgs<Entity>
    {
        public EntityEventArgs(IMapTransactionWithMoveSupport transaction)
            : base(transaction)
        {
        }
    }

    public class TileEventArgs : GameEventArgs<Tile>
    {
        public TileEventArgs(IMapTransactionWithMoveSupport transaction)
            : base(transaction)
        {
        }
    }

    public class FollowUpEventArgs
    {
        private EntityMoveTransaction _completedTransaction;

        public IReadOnlyMapTransaction CompletedTransaction => _completedTransaction;

        public FollowUpEventArgs(EntityMoveTransaction completedTransaction)
        {
            _completedTransaction = completedTransaction;
        }

        public IMapTransactionWithMoveSupport CreateFollowUpTransaction()
        {
            return new EntityMoveTransaction((EntityMoveTransaction)_completedTransaction.Last);
        }
    }
}
