using OrangeBugReloaded.Core.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Transactions
{
    /// <summary>
    /// A transaction that stores information about entity moves
    /// that are executed in its context.
    /// </summary>
    public interface ITransactionWithMoveSupport : ITransaction<TileInfo>
    {
        /// <summary>
        /// Provides information about each move in a recursion of move operations.
        /// The topmost element represents the current move.
        /// </summary>
        Stack<EntityMoveInfo> Moves { get; }

        /// <summary>
        /// Provides information about the current entity move operation.
        /// This is equivalent to the topmost element of <see cref="Moves"/>.
        /// </summary>
        EntityMoveInfo CurrentMove { get; }

        /// <summary>
        /// Applies the changes collected in the transaction to the specified
        /// map and flushes events to the specified observer.
        /// </summary>
        /// <param name="map">Map</param>
        /// <param name="version">The version to use for the changed tiles</param>
        /// <param name="eventSource">Observer</param>
        /// <returns>Task</returns>
        Task CommitAsync(IMap map, int version, IObserver<IGameEvent> eventSource);
    }
}
