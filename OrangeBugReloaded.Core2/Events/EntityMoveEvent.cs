using System.Collections.Generic;

namespace OrangeBugReloaded.Core.Events
{
    public class EntityMoveEvent : ILocatedGameEvent
    {
        public Point SourcePosition { get; }
        public Point TargetPosition { get; }
        public Tile Source { get; }
        public Tile Target { get; }

        public EntityMoveEvent(Point sourcePosition, Point targetPosition, Tile source, Tile target)
        {
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
            Source = source;
            Target = target;
        }

        public IEnumerable<Point> GetPositions()
        {
            yield return SourcePosition;
            yield return TargetPosition;
        }
    }
}
