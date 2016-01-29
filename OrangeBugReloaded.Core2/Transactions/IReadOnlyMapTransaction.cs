using OrangeBugReloaded.Core.Events;
using System.Collections.Generic;

namespace OrangeBugReloaded.Core.Transactions
{
    /// <summary>
    /// The base interface for transactions.
    /// <para>
    /// Transactions are objects that simulate certain actions on a map
    /// and record changes that would be made to the map.
    /// Eventually if the transaction is not cancelled these changes are
    /// committed, i.e. applied to the underlying map, in an atomic way.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Transactions can be chained together.
    /// This way a transaction can override changes of earlier transactions.
    /// </remarks>
    public interface IReadOnlyMapTransaction : IReadOnlyMap, IGameEventEmitter
    {
        /// <summary>
        /// Indicates whether the transaction has been cancelled.
        /// Changes of a cancelled transaction are not committed and therefore
        /// not applied to the map.
        /// Cancelled transactions cannot initiate further move operations.
        /// Follow-up transactions cannot be created for a canclled transaction.
        /// </summary>
        bool IsCancelled { get; }
        
        /// <summary>
        /// An arbitrary tag value that can be used to mark the object that
        /// has caused the chain of transactions.
        /// This value is shared across all transactions within a chain.
        /// </summary>
        object Initiator { get; set; }

        IEnumerable<KeyValuePair<Point, Tile>> ChangedTiles { get; }

        /// <summary>
        /// Gets the previous transaction in the chain.
        /// </summary>
        IReadOnlyMapTransaction Previous { get; }

        /// <summary>
        /// Gets the next transaction in the chain.
        /// </summary>
        IReadOnlyMapTransaction Next { get; }

        /// <summary>
        /// Gets the last (latest) transaction in the chain.
        /// </summary>
        IReadOnlyMapTransaction Last { get; }

        /// <summary>
        /// Gets the first (oldest) transaction in the chain.
        /// </summary>
        IReadOnlyMapTransaction First { get; }
    }
}
