using OrangeBugReloaded.Core.Transactions;
using System;
using System.Collections.Generic;

namespace OrangeBugReloaded.Core
{
    public class ScheduledMove
    {
        /// <summary>
        /// The tile (and its position) that has scheduled the move.
        /// </summary>
        public MoveInitiator Initiator { get; }

        public Point SourcePosition { get; }

        public Point TargetPosition { get; }

        public DateTimeOffset ExecutionTime { get; }

        public ScheduledMove(MoveInitiator initiator, Point sourcePosition, Point targetPosition, DateTimeOffset executionTime)
        {
            Initiator = initiator;
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
            ExecutionTime = executionTime;
        }
    }
}
