namespace OrangeBugReloaded.Core.Events
{
    public class EntityMoveEvent : IGameEvent
    {
        public Point SourcePosition { get; }
        public Point TargetPosition { get; }
        public Entity SourceEntity { get; }
        public Entity TargetEntity { get; }

        public EntityMoveEvent(Point sourcePosition, Point targetPosition, Entity sourceEntity, Entity targetEntity)
        {
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
            SourceEntity = sourceEntity;
            TargetEntity = targetEntity;
        }
    }
}
