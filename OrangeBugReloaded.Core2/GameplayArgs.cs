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
    public class GameplayArgs : IBeginMoveArgs, IAttachArgs, IDetachArgs, IMovesCompletedArgs, IFollowUpArgs, IEntityDetachArgs
    {
        private readonly List<FollowUpEvent> _followUpEvents = new List<FollowUpEvent>();
        private readonly ITransactionWithMoveSupport _transaction;
        private readonly IMap _map;

        public Tile Tile { get; }
        public Point SuggestedPushDirection { get; }
        public bool IsSealed => _transaction.IsSealed;
        public EntityMoveInfo CurrentMove => _transaction.CurrentMove;
        public IReadOnlyCollection<FollowUpEvent> FollowUpEvents => _followUpEvents;
        public Tile Result { get; set; }
        public Entity ResultingEntity { get; set; }

        public MoveInitiator Initiator
        {
            get { return _transaction.Initiator; }
            set { _transaction.Initiator = value; }
        }

        public GameplayArgs(ITransactionWithMoveSupport transaction, IMap map)
        {
            _transaction = transaction;
            _map = map;
        }

        private GameplayArgs(ITransactionWithMoveSupport transaction, IGameplayMap map, Tile tile, Point suggestedPushDirection)
            : this(transaction, map)
        {
            Tile = tile;
            SuggestedPushDirection = suggestedPushDirection;
        }

        public void Seal() => _transaction.IsSealed = true;

        public void Emit(IGameEvent e) => _transaction.Emit(e);

        public async Task<TileInfo> GetAsync(Point position)
        {
            // Try to get from transaction first.
            // If that fails, load directly from map.
            var tileInfo = _transaction.Changes.TryGetValue(position);
            return (tileInfo != TileInfo.Empty) ? tileInfo : await _map.GetAsync(position);
        }

        Task<TileMetadata> IReadOnlyMap.GetMetadataAsync(Point position)
            => _map.GetMetadataAsync(position);

        IEntityDetachArgs IAttachArgs.CreateEntityDetachArgs(Tile tile, Point suggestedPushDirection)
            => new GameplayArgs(_transaction, (IGameplayMap)_map, tile, suggestedPushDirection);

        public async Task<bool> MoveAsync(Point sourcePosition, Point targetPosition)
        {
            var result = await ((IGameplayMap)_map).MoveAsync(sourcePosition, targetPosition, _transaction);
            _followUpEvents.AddRange(result.FollowUpEvents); // TODO: Could we get duplicates here?
            return result.IsSuccessful;
        }

        public void ValidateResult()
        {
            if ((Result == null && ResultingEntity == null) && !_transaction.IsSealed)
                throw new InvalidOperationException("Invalid result: No result is provided and the transaction is not sealed");

            if ((Result != null || ResultingEntity != null) && _transaction.IsSealed)
                throw new InvalidOperationException("Invalid result: The transaction is sealed but a result is provided");
        }
    }

    // Interfaces for all the different events on tiles and entities

    public interface IBeginMoveArgs : IReadOnlyMap, IHasInitiator, ICurrentMoveAware, ISealable, IGameEventEmitter
    {
        Entity ResultingEntity { get; set; }
        void ValidateResult();
    }

    public interface IAttachArgs : IReadOnlyMap, IHasInitiator, ICurrentMoveAware, ISealable, IGameEventEmitter, ISupportsMove
    {
        Tile Result { get; set; }
        IEntityDetachArgs CreateEntityDetachArgs(Tile tile, Point suggestedPushDirection);
        void ValidateResult();
    }

    public interface IDetachArgs : IReadOnlyMap, IHasInitiator, ICurrentMoveAware, ISealable, IGameEventEmitter, ISupportsMove
    {
        Tile Result { get; set; }
        void ValidateResult();
    }

    public interface IMovesCompletedArgs : IReadOnlyMap, IHasInitiator, IGameEventEmitter
    {
        Tile Result { get; set; }
        void ValidateResult();
    }

    public interface IFollowUpArgs : IReadOnlyMap, IGameEventEmitter, ISupportsMove
    {
        MoveInitiator Initiator { get; set; }
        IReadOnlyCollection<FollowUpEvent> FollowUpEvents { get; }
    }

    public interface IEntityDetachArgs : IReadOnlyMap, IHasInitiator, ICurrentMoveAware, ISealable, IGameEventEmitter, ISupportsMove
    {
        Tile Tile { get; }
        Point SuggestedPushDirection { get; }
    }

    // Base interfaces

    public interface IHasInitiator
    {
        MoveInitiator Initiator { get; }
    }

    public interface ICurrentMoveAware
    {
        EntityMoveInfo CurrentMove { get; }
    }

    public interface ISealable
    {
        bool IsSealed { get; }
        void Seal();
    }

    public interface ISupportsMove
    {
        Task<bool> MoveAsync(Point sourcePosition, Point targetPosition);
    }
}
