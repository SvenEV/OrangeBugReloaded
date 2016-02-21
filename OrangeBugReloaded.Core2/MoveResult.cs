using OrangeBugReloaded.Core.Transactions;
using System.Collections.Generic;
using System.Linq;

namespace OrangeBugReloaded.Core
{
    public class MoveResult
    {
        /// <summary>
        /// Indicates whether the move has been successful.
        /// This is the case if the entity has correctly been detached
        /// from the source tile and attached to the target tile.
        /// </summary>
        public bool IsSuccessful { get; }

        /// <summary>
        /// The transaction under which the move has been executed.
        /// This is the transaction that has recorded the changes caused
        /// by the move.
        /// </summary>
        public ITransactionWithMoveSupport Transaction { get; }

        /// <summary>
        /// Follow-up events that have been created during the move.
        /// These have to be scheduled and executed at the designated times.
        /// </summary>
        public IReadOnlyCollection<FollowUpEvent> FollowUpEvents { get; }

        public MoveResult(ITransactionWithMoveSupport transaction, bool isSuccessful, IEnumerable<FollowUpEvent> followUpEvents)
        {
            Transaction = transaction;
            IsSuccessful = isSuccessful;
            FollowUpEvents = followUpEvents?.ToArray() ?? new FollowUpEvent[0];
        }
    }

    public class AreaSpawnResult : MoveResult
    {
        public Point SpawnPosition { get; }

        public AreaSpawnResult(ITransactionWithMoveSupport transaction, bool isSuccessful, IEnumerable<FollowUpEvent> followUpEvents, Point spawnPosition)
            : base(transaction, isSuccessful, followUpEvents)
        {
            SpawnPosition = spawnPosition;
        }
    }
}
