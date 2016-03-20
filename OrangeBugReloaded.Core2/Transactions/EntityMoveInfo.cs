namespace OrangeBugReloaded.Core.Transactions
{
    public class EntityMoveInfo
    {
        public Entity Entity { get; set; }
        public Point SourcePosition { get; set; }
        public Point TargetPosition { get; set; }

        public Point Offset => TargetPosition - SourcePosition;

        public EntityMoveInfo(Entity entity, Point sourcePosition, Point targetPosition)
        {
            Entity = entity;
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
        }
    }
}
