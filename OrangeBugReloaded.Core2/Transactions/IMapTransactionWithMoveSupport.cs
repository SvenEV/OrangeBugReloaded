using System.Collections.Generic;

namespace OrangeBugReloaded.Core.Transactions
{
    /// <summary>
    /// A transaction with the ability to perform entity move operations.
    /// </summary>
    public interface IMapTransactionWithMoveSupport : IReadOnlyMapTransaction, ISupportsMove
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
        /// Cancels the transaction.
        /// <seealso cref="IReadOnlyMapTransaction"/>
        /// </summary>
        void Cancel();
    }
}
