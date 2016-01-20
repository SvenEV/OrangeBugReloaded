using OrangeBugReloaded.Core.Foundation;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Defines the behavior of a trigger that can be turned off or on.
    /// </summary>
    public interface ITrigger : IBindable
    {
        /// <summary>
        /// Indicates whether the trigger is turned on.
        /// </summary>
        bool IsOn { get; }
    }
}
