namespace OrangeBugReloaded.Core.Events
{
    public class EntitySpawnEvent : IGameEvent
    {
        public Point Position { get; }
        public Entity Entity { get; }

        public EntitySpawnEvent(Point position, Entity entity)
        {
            Position = position;
            Entity = entity;
        }
    }
}
