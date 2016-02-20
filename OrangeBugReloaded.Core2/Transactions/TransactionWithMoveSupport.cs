using System;
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
        public static TransactionWithMoveSupport EmptyFinalizedTransaction { get; }

        static TransactionWithMoveSupport()
        {
            EmptyFinalizedTransaction = new TransactionWithMoveSupport(MoveInitiator.Empty);
            EmptyFinalizedTransaction.StopRecording();
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

        public async Task CommitAsync(IMap map, int version, IObserver<IGameEvent> eventSource)
        {
            // Apply recorded changes to map
            foreach (var kvp in Changes)
                await map.SetAsync(kvp.Key, kvp.Value.WithVersion(version));

            // Flush recorded events
            foreach (var e in Events)
                eventSource?.OnNext(e);
        }
    }
}
