namespace OrangeBugReloaded.Core.Events
{
    public class EntityDespawnEvent : IGameEvent
    {
        public Point Position { get; }
        public Entity Entity { get; }

        public EntityDespawnEvent(Point position, Entity entity)
        {
            Position = position;
            Entity = entity;
        }
    }
}
