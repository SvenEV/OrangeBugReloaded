using System;
using System.Collections.Generic;

namespace OrangeBugReloaded.Core.Events
{
    public class EntitySpawnEvent : ILocatedGameEvent
    {
        public Point Position { get; }
        public Entity Entity { get; }

        public EntitySpawnEvent(Point position, Entity entity)
        {
            Position = position;
            Entity = entity;
        }

        public IEnumerable<Point> GetPositions()
        {
            yield return Position;
        }
    }
}
