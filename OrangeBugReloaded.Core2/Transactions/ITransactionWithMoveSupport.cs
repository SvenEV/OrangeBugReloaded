using System.Collections.Generic;

namespace OrangeBugReloaded.Core.Transactions
{
    /// <summary>
    /// A transaction that stores information about entity moves
    /// that are executed in its context.
    /// </summary>
    public interface ITransactionWithMoveSupport : ITransaction
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
    }
}
