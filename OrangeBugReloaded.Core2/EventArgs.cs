using OrangeBugReloaded.Core.Transactions;
using System;

namespace OrangeBugReloaded.Core
{
    public abstract class GameEventArgs<TResult, TTransaction> where TTransaction : IReadOnlyMapTransaction
    {
        public TTransaction Transaction { get; }

        public TResult Result { get; set; }

        public GameEventArgs(TTransaction transaction)
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

    public class EntityEventArgs : GameEventArgs<Entity, IMapTransactionWithMoveSupport>
    {
        public EntityEventArgs(IMapTransactionWithMoveSupport transaction)
            : base(transaction)
        {
        }
    }

    public class AttachEventArgs : TileEventArgs
    {
        /// <summary>
        /// If set to true, the detachment of the entity from the source tile
        /// is not executed. Note that this may duplicate an entity if the
        /// target tile decides to correctly attach the entity.
        /// </summary>
        public bool PreventDetach { get; set; }

        public AttachEventArgs(IMapTransactionWithMoveSupport transaction) : base(transaction)
        {
        }
    }

    public class DetachEventArgs : TileEventArgs
    {
        /// <summary>
        /// If set to true, the attachment of the entity to the target tile
        /// is not executed.
        /// </summary>
        public bool PreventAttach { get; set; }

        public DetachEventArgs(IMapTransactionWithMoveSupport transaction) : base(transaction)
        {
        }
    }

    public class TileEventArgs : GameEventArgs<Tile, IMapTransactionWithMoveSupport>
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
