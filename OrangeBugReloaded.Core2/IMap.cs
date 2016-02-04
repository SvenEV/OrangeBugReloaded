using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Transactions;
using System;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// A map that supports get-operations.
    /// </summary>
    public interface IReadOnlyMap
    {
        /// <summary>
        /// Gets the tile at the specified position.
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="layer">Layer</param>
        /// <returns>The <see cref="Tile"/></returns>
        Task<Tile> GetAsync(Point position, MapLayer layer = MapLayer.Gameplay);
    }

    /// <summary>
    /// A map that supports get- and set-operations.
    /// </summary>
    public interface IMap : IReadOnlyMap
    {
        /// <summary>
        /// Sets the tile at the specified position.
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="tile">Tile</param>
        /// <param name="layer">Layer</param>
        /// <returns>True if the tile has changed during the call</returns>
        Task<bool> SetAsync(Point position, Tile tile, MapLayer layer = MapLayer.Gameplay);
    }

    public interface ISupportsMove
    {
        /// <summary>
        /// Tries to move the <see cref="Entity"/> at the specified
        /// source position to the specified target position.
        /// </summary>
        /// <param name="sourcePosition">The position from where the entity is moved</param>
        /// <param name="targetPosition">The position to which the entity is moved</param>
        /// <returns>
        /// True if the inital move transaction was successful (follow-up transactions might have failed)
        /// </returns>
        Task<bool> MoveAsync(Point sourcePosition, Point targetPosition);
    }

    /// <summary>
    /// A map that supports get-, set- and move-operations and
    /// provides a stream of gameplay events.
    /// </summary>
    public interface IGameplayMap : IMap, ISupportsMove
    {
        /// <summary>
        /// Provides a stream of events related to the game.
        /// These events provide information about chunks being loaded
        /// or unloaded, entity moves that are executed etc.
        /// </summary>
        IObservable<IGameEvent> Events { get; }

        /// <summary>
        /// Tries to move the <see cref="Entity"/> at the specified
        /// source position to the specified target position
        /// using an existing transaction.
        /// </summary>
        /// <param name="sourcePosition">The position from where the entity is moved</param>
        /// <param name="targetPosition">The position to which the entity is moved</param>
        /// <param name="transactionChain">The transaction chain that records the changes made during the move</param>
        /// <returns>
        /// True if the inital move transaction was successful (follow-up transactions might have failed)
        /// </returns>
        Task<bool> MoveAsync(Point sourcePosition, Point targetPosition, ITransactionChainWithMoveSupport transactionChain);
    }
}
