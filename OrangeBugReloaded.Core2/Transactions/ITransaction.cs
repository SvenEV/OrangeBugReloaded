using OrangeBugReloaded.Core.Events;
using System.Collections.Generic;

namespace OrangeBugReloaded.Core.Transactions
{
    /// <summary>
    /// The base interface for transactions.
    /// <para>
    /// Transactions are objects that simulate certain actions on a map
    /// and record changes that would be made to the map.
    /// Eventually these changes are committed, i.e. applied to the
    /// underlying map, in an atomic way.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of changes</typeparam>
    /// <typeparam name="TMap">The type of the underlying map</typeparam>
    public interface ITransaction<T> : IGameEventEmitter
    {
        IReadOnlyDictionary<Point, T> Changes { get; }

        IReadOnlyList<IGameEvent> Events { get; }

        /// <summary>
        /// Provides information about the object that caused the moves.
        /// </summary>
        MoveInitiator Initiator { get; set; }

        /// <summary>
        /// Indicates whether recording of changes has stopped.
        /// If this is true, the transaction won't allow further changes
        /// or events to be recorded.
        /// </summary>
        /// <remarks>
        /// Setting this from true to false should only happen
        /// in very rare well documented situations.
        /// </remarks>
        bool IsSealed { get; set; }

        /// <summary>
        /// Adds a specific change to the transaction.
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="oldValue">
        /// Old value that is already existent outside the transaction
        /// </param>
        /// <param name="value">New value</param>
        /// <returns>
        /// True if the new value has been added to the transaction.
        /// False otherwise; that is if the old value equals the new value
        /// or if the transaction is sealed.
        /// </returns>
        bool Set(Point position, T oldValue, T value);
    }
}
