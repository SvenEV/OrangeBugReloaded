using System.Collections.Generic;

namespace OrangeBugReloaded.Core.Events
{
    /// <summary>
    /// The event that is emitted when the entity changes during
    /// <see cref="Tile.OnEntityMoveTransactionCompletedAsync(IMovesCompletedArgs)"/>.
    /// </summary>
    public class EntityChangeEvent : ILocatedGameEvent
    {
        public Point Position { get; }
        public Entity Entity { get; }

        public EntityChangeEvent(Point position, Entity entity)
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
