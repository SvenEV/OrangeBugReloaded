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
    /// <typeparam name="T">The type of the transaction in the chain</typeparam>
    /// <typeparam name="TMap">The type of the underlying map</typeparam>
    public interface ITransaction
    {
        /// <summary>
        /// Indicates whether the transaction has been cancelled.
        /// Changes of a cancelled transaction are not committed and therefore
        /// not applied to the map.
        /// Cancelled transactions cannot initiate further move operations.
        /// Follow-up transactions cannot be created for a canclled transaction.
        /// </summary>
        bool IsCancelled { get; }

        IDictionary<Point, Tile> ChangedTiles { get; }

        IList<IGameEvent> Events { get; }

        /// <summary>
        /// Cancels the transaction.
        /// <seealso cref="IsCancelled"/>
        /// </summary>
        void Cancel();
    }
}
