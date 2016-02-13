using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Transactions;
using System;
using System.Collections.Generic;
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
        protected readonly ITransactionWithMoveSupport _transaction;
        protected readonly IMap _map;

        public TResult Result { get; set; }

        public EntityMoveInfo CurrentMove => _transaction.CurrentMove;

        public bool IsCanceled => _transaction.IsCanceled;

        public object Initiator => _transaction.Initiator;

        public GameEventArgs(ITransactionWithMoveSupport transaction, IMap map)
        {
            _transaction = transaction;
            _map = map;
        }

        public void Cancel() => _transaction.Cancel();

        public void Emit(IGameEvent e)
        {
            // TODO
            //_transaction.Emit(e);
        }

        public Task<TileInfo> GetAsync(Point position)
            => _map.GetAsync(position);

        Task<TileMetadata> IReadOnlyMap.GetMetadataAsync(Point position)
            => _map.GetMetadataAsync(position);

        internal void ValidateResult()
        {
            if (Result == null && !_transaction.IsCanceled)
                throw new InvalidOperationException("Invalid result: No result is provided and the transaction is not cancelled");

            if (Result != null && _transaction.IsCanceled)
                throw new InvalidOperationException("Invalid result: The transaction is cancelled but a result is provided");
        }
    }

    public class EntityEventArgs : GameEventArgs<Entity>
    {
        public EntityEventArgs(ITransactionWithMoveSupport transaction, IMap map)
            : base(transaction, map)
        {
        }
    }

    public class TileEventArgs : GameEventArgs<Tile>, ISupportsMove
    {
        private readonly new IGameplayMap _map;

        public TileEventArgs(ITransactionWithMoveSupport transaction, IGameplayMap map)
            : base(transaction, map)
        {
            _map = map;
        }

        public Task<MoveResult> MoveAsync(Point sourcePosition, Point targetPosition)
            => _map.MoveAsync(sourcePosition, targetPosition, _transaction);
    }

    public class AttachEventArgs : TileEventArgs
    {
        /// <summary>
        /// If set to true, the detachment of the entity from the source tile
        /// is not executed. Note that this may duplicate an entity if the
        /// target tile decides to correctly attach the entity.
        /// </summary>
        public bool PreventDetach { get; set; }

        public AttachEventArgs(ITransactionWithMoveSupport transaction, IGameplayMap map)
            : base(transaction, map)
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

        public DetachEventArgs(ITransactionWithMoveSupport transaction, IGameplayMap map)
            : base(transaction, map)
        {
        }
    }

    public class FollowUpEventArgs : IReadOnlyMap, IGameEventEmitter
    {
        private List<ScheduledMove> _followUpMoves = new List<ScheduledMove>();
        private readonly IGameplayMap _map;

        public MoveInitiator Initiator { get; }
        public IReadOnlyCollection<ScheduledMove> FollowUpMoves => _followUpMoves;

        public FollowUpEventArgs(IGameplayMap map, MoveInitiator initiator)
        {
            _map = map;
            Initiator = initiator;
        }
        
        public void ScheduleMove(MoveInitiator initiator, Point sourcePosition, Point targetPosition, DateTimeOffset executionTime)
        {
            if (executionTime < DateTimeOffset.Now)
                throw new ArgumentException("Cannot schedule a move in the past");

            _followUpMoves.Add(new ScheduledMove(initiator, sourcePosition, targetPosition, executionTime));
        }

        public void Emit(IGameEvent e)
        {
            //_followUpTransaction.Emit(e); // TODO
        }

        public Task<TileInfo> GetAsync(Point position)
            => _map.GetAsync(position);

        Task<TileMetadata> IReadOnlyMap.GetMetadataAsync(Point position)
            => _map.GetMetadataAsync(position);
    }
}
