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

        public async Task<TileInfo> GetAsync(Point position)
        {
            // Try to get from transaction first.
            // If that fails, load directly from map.
            var tileInfo = _transaction.Changes.TryGetValue(position);
            return (tileInfo != TileInfo.Empty) ? tileInfo : await _map.GetAsync(position);
        }

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

    public class EntityBeginMoveArgs : GameEventArgs<Entity>
    {
        public EntityBeginMoveArgs(ITransactionWithMoveSupport transaction, IMap map)
            : base(transaction, map)
        {
        }
    }

    public class TileEventArgs : GameEventArgs<Tile>, ISupportsMove
    {
        protected readonly new IGameplayMap _map;

        public TileEventArgs(ITransactionWithMoveSupport transaction, IGameplayMap map)
            : base(transaction, map)
        {
            _map = map;
        }

        public Task<MoveResult> MoveAsync(Point sourcePosition, Point targetPosition)
            => _map.MoveAsync(sourcePosition, targetPosition, _transaction);
    }

    public class AttachArgs : TileEventArgs
    {
        /// <summary>
        /// If set to true, the detachment of the entity from the source tile
        /// is not executed. Note that this may duplicate an entity if the
        /// target tile decides to correctly attach the entity.
        /// </summary>
        public bool PreventDetach { get; set; }

        public AttachArgs(ITransactionWithMoveSupport transaction, IGameplayMap map)
            : base(transaction, map)
        {
        }

        public EntityDetachArgs CreateEntityDetachArgs(Tile tile, Point pushDirection)
            => new EntityDetachArgs(_transaction, _map, tile, pushDirection);
    }

    public class DetachArgs : TileEventArgs
    {
        /// <summary>
        /// If set to true, the attachment of the entity to the target tile
        /// is not executed.
        /// </summary>
        public bool PreventAttach { get; set; }

        public DetachArgs(ITransactionWithMoveSupport transaction, IGameplayMap map)
            : base(transaction, map)
        {
        }
    }

    public class EntityDetachArgs : TileEventArgs
    {
        /// <summary>
        /// The tile with the entity to be detached.
        /// </summary>
        public Tile Tile { get; }

        /// <summary>
        /// The push direction that is suggested for pushable entities.
        /// </summary>
        public Point SuggestedPushDirection { get; }

        public EntityDetachArgs(ITransactionWithMoveSupport transaction, IGameplayMap map, Tile tile, Point pushDirection)
            : base(transaction, map)
        {
            Tile = tile;
            SuggestedPushDirection = pushDirection.EnsureDirection();
        }
    }

    public class FollowUpEventArgs : IReadOnlyMap, IGameEventEmitter
    {
        private List<FollowUpEvent> _followUpEvents = new List<FollowUpEvent>();
        private readonly ITransactionWithMoveSupport _transaction;
        private readonly IGameplayMap _map;

        public MoveInitiator Initiator => _transaction.Initiator;
        public IReadOnlyCollection<FollowUpEvent> FollowUpEvents => _followUpEvents;

        public FollowUpEventArgs(IGameplayMap map, ITransactionWithMoveSupport transaction)
        {
            _map = map;
            _transaction = transaction;
        }

        public async Task<bool> MoveAsync(Point sourcePosition, Point targetPosition)
        {
            var result = await _map.MoveAsync(sourcePosition, targetPosition, _transaction);
            _followUpEvents.AddRange(result.FollowUpEvents); // TODO: Could we get duplicates here?
            return result.IsSuccessful;
        }

        public void Emit(IGameEvent e)
        {
            //_followUpTransaction.Emit(e); // TODO
        }

        public async Task<TileInfo> GetAsync(Point position)
        {
            // Try to get from transaction first.
            // If that fails, load directly from map.
            var tileInfo = _transaction.Changes.TryGetValue(position);
            return (tileInfo != TileInfo.Empty) ? tileInfo : await _map.GetAsync(position);
        }

        Task<TileMetadata> IReadOnlyMap.GetMetadataAsync(Point position)
            => _map.GetMetadataAsync(position);
    }
}
