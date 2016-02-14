using OrangeBugReloaded.Core.Transactions;
using System;

namespace OrangeBugReloaded.Core
{
    [Obsolete("Use FollowUpEvent instead", true)]
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

    public class FollowUpEvent
    {
        public Point Position { get; }
        public MoveInitiator Initiator { get; }
        public DateTimeOffset ExecutionTime { get; }

        public FollowUpEvent(Point position, MoveInitiator initiator, DateTimeOffset executionTime)
        {
            Position = position;
            Initiator = initiator;
            ExecutionTime = executionTime;
        }

        public override string ToString() => $"{Position} scheduled for {ExecutionTime}";
    }
}
