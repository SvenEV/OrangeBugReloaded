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
        /// <summary>
        /// Indicates whether recording of changes has stopped.
        /// If this is true, the transaction won't allow further changes
        /// or events to be recorded.
        /// </summary>
        bool IsFinalized { get; }

        IReadOnlyDictionary<Point, T> Changes { get; }

        IReadOnlyList<IGameEvent> Events { get; }

        /// <summary>
        /// Provides information about the object that caused the moves.
        /// </summary>
        MoveInitiator Initiator { get; set; }

        bool Set(Point position, T oldValue, T value);

        /// <summary>
        /// Finalizes the transaction so that no more
        /// changes can be added.
        /// <seealso cref="IsFinalized"/>
        /// </summary>
        void StopRecording();
    }
}
