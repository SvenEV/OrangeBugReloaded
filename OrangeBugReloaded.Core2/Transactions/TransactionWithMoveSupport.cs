﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrangeBugReloaded.Core.Events;

namespace OrangeBugReloaded.Core.Transactions
{
    /// <summary>
    /// A transaction that can simulate entity move operations and record changes
    /// caused by such operations.
    /// </summary>
    public class TransactionWithMoveSupport : TransactionBase<TileInfo>, ITransactionWithMoveSupport
    {
        public static TransactionWithMoveSupport EmptySealedTransaction { get; }

        static TransactionWithMoveSupport()
        {
            EmptySealedTransaction = new TransactionWithMoveSupport(MoveInitiator.Empty);
            EmptySealedTransaction.IsSealed = true;
        }

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

        public TransactionWithMoveSupport(MoveInitiator initiator) : base(initiator)
        {
        }

        public async Task CommitAsync(IMap map, int version)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            // Apply recorded changes to map
            foreach (var kvp in Changes)
                await map.SetAsync(kvp.Key, kvp.Value.WithVersion(version));

            // Flush recorded events
            foreach (var e in Events)
                (map as IGameEventEmitter)?.Emit(e);
        }
    }
}
