namespace OrangeBugReloaded.Core.Entities
{
    /// <summary>
    /// The entity type that represents a non-existent entity.
    /// </summary>
    public class NullEntity : Entity
    {
        public static NullEntity Default { get; } = new NullEntity();
    }
}
