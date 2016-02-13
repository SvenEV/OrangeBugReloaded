using System.Collections.Generic;
using System.Linq;

namespace OrangeBugReloaded.Core.ClientServer
{
    public class RemoteMoveRequest
    {
        public Point SourcePosition { get; }

        public Point TargetPosition { get; }

        public bool IsSuccessful { get; }

        /// <summary>
        /// The positions and versions of tiles that are affected
        /// by the execution of the move on the client.
        /// This is empty if the transaction is canceled.
        /// </summary>
        public IReadOnlyCollection<VersionedPoint> AffectedPositions { get; }

        private RemoteMoveRequest(Point sourcePosition, Point targetPosition, bool isSuccessful, IEnumerable<VersionedPoint> affectedPositions)
        {
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
            IsSuccessful = isSuccessful;
            AffectedPositions = affectedPositions.ToArray();
        }

        public static RemoteMoveRequest CreateSuccessful(VersionedPoint sourcePosition, VersionedPoint targetPosition, IEnumerable<VersionedPoint> affectedPositions)
            // Note that sourcePosition and targetPosition are always included in affectedPositions if the move is successful
            => new RemoteMoveRequest(sourcePosition.Position, targetPosition.Position, true, affectedPositions);

        public static RemoteMoveRequest CreateFaulted(VersionedPoint sourcePosition, VersionedPoint targetPosition)
            => new RemoteMoveRequest(sourcePosition.Position, targetPosition.Position, false, new[] { sourcePosition, targetPosition });
    }
}
