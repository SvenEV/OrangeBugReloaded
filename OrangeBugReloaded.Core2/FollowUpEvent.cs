using OrangeBugReloaded.Core.Transactions;
using System;

namespace OrangeBugReloaded.Core
{
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
