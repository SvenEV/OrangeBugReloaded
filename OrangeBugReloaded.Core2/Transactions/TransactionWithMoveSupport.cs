using System;
using System.Collections.Generic;

namespace OrangeBugReloaded.Core.Transactions
{
    /// <summary>
    /// A transaction that can simulate entity move operations and record changes
    /// caused by such operations.
    /// </summary>
    public class TransactionWithMoveSupport : Transaction, ITransactionWithMoveSupport
    {
        /// <inheritdoc/>
        public Stack<EntityMoveInfo> Moves { get; } = new Stack<EntityMoveInfo>();

        /// <inheritdoc/>
        public EntityMoveInfo CurrentMove
        {
            get
            {
                if (Moves.Count == 0)
                    throw new InvalidOperationException($"{nameof(CurrentMove)} is not available in this context");

                return Moves.Peek();
            }
        }
    }
}
