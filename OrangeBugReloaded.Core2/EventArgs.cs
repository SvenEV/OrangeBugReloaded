using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Transactions;
using System;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// A facade for tile and entity events that provides information
    /// about the current transaction and the current entity move.
    /// </summary>
    /// <typeparam name="TResult">Result type</typeparam>
    public abstract class GameEventArgs<TResult> : IReadOnlyMap, IGameEventEmitter
    {
        protected readonly ITransactionChainWithMoveSupport _transactionChain;

        public TResult Result { get; set; }

        public EntityMoveInfo CurrentMove => _transactionChain.CurrentTransaction.CurrentMove;

        public bool IsCanceled => _transactionChain.CurrentTransaction.IsCanceled;

        public object Initiator
        {
            get { return _transactionChain.Initiator; }
            set { _transactionChain.Initiator = value; }
        }

        public GameEventArgs(ITransactionChainWithMoveSupport transactionChain)
        {
            _transactionChain = transactionChain;
        }

        public void Cancel() => _transactionChain.CurrentTransaction.Cancel();

        public void Emit(IGameEvent e) => _transactionChain.Emit(e);

        public Task<Tile> GetAsync(Point position)
            => _transactionChain.GetAsync(position);

        Task<TileMetadata> IReadOnlyMap.GetMetadataAsync(Point position)
            => _transactionChain.GetMetadataAsync(position);

        internal void ValidateResult()
        {
            if (Result == null && !_transactionChain.CurrentTransaction.IsCanceled)
                throw new InvalidOperationException("Invalid result: No result is provided and the transaction is not cancelled");

            if (Result != null && _transactionChain.CurrentTransaction.IsCanceled)
                throw new InvalidOperationException("Invalid result: The transaction is cancelled but a result is provided");
        }
    }

    public class EntityEventArgs : GameEventArgs<Entity>
    {
        public EntityEventArgs(ITransactionChainWithMoveSupport transactionChain) : base(transactionChain)
        {
        }
    }

    public class TileEventArgs : GameEventArgs<Tile>, ISupportsMove
    {
        public TileEventArgs(ITransactionChainWithMoveSupport transactionChain) : base(transactionChain)
        {
        }

        public Task<bool> MoveAsync(Point sourcePosition, Point targetPosition)
            => _transactionChain.MoveAsync(sourcePosition, targetPosition);
    }

    public class AttachEventArgs : TileEventArgs
    {
        /// <summary>
        /// If set to true, the detachment of the entity from the source tile
        /// is not executed. Note that this may duplicate an entity if the
        /// target tile decides to correctly attach the entity.
        /// </summary>
        public bool PreventDetach { get; set; }

        public AttachEventArgs(ITransactionChainWithMoveSupport transactionChain) : base(transactionChain)
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

        public DetachEventArgs(ITransactionChainWithMoveSupport transactionChain) : base(transactionChain)
        {
        }
    }

    public class FollowUpEventArgs : ISupportsMove, IReadOnlyMap, IGameEventEmitter
    {
        private readonly ITransactionChainWithMoveSupport _transactionChain;
        private bool _followUpTransactionCreated = false;

        public object Initiator
        {
            get { return _transactionChain.Initiator; }
            set { _transactionChain.Initiator = value; }
        }

        public FollowUpEventArgs(ITransactionChainWithMoveSupport transactionChain)
        {
            _transactionChain = transactionChain;
        }

        public async Task<bool> MoveAsync(Point sourcePosition, Point targetPosition)
        {
            EnsureFollowUpTransactionCreated();
            return await _transactionChain.MoveAsync(sourcePosition, targetPosition);
        }

        public void Emit(IGameEvent e)
        {
            EnsureFollowUpTransactionCreated();
            _transactionChain.Emit(e);
        }

        public Task<Tile> GetAsync(Point position)
            => _transactionChain.GetAsync(position);

        Task<TileMetadata> IReadOnlyMap.GetMetadataAsync(Point position)
            => _transactionChain.GetMetadataAsync(position);

        private void EnsureFollowUpTransactionCreated()
        {
            if (!_followUpTransactionCreated)
            {
                _transactionChain.CreateFollowUpTransaction();
                _followUpTransactionCreated = true;
            }
        }
    }
}
