using System.Collections.Generic;
using System.Linq;

namespace OrangeBugReloaded.Core.ClientServer
{
    public class RemoteMoveRequest
    {
        public VersionedPoint SourcePosition { get; }

        public VersionedPoint TargetPosition { get; }

        /// <summary>
        /// The positions and versions of tiles that are affected
        /// by the execution of the move on the client.
        /// </summary>
        public IReadOnlyCollection<VersionedPoint> AffectedPositions { get; }

        public RemoteMoveRequest(VersionedPoint sourcePosition, VersionedPoint targetPosition, IEnumerable<VersionedPoint> affectedPositions)
        {
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
            AffectedPositions = affectedPositions.ToArray();
        }
    }
}
