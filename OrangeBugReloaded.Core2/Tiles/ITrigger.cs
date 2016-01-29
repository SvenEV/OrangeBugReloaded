namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// Defines the behavior of a trigger that can be turned on and off.
    /// </summary>
    public interface ITrigger
    {
        /// <summary>
        /// Indicates whether the trigger is turned on.
        /// </summary>
        bool IsOn { get; }
    }
}
