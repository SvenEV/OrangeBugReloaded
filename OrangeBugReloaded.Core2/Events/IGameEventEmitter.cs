namespace OrangeBugReloaded.Core.Events
{
    public interface IGameEventEmitter
    {
        /// <summary>
        /// Raises an event.
        /// </summary>
        /// <param name="e">The event arguments</param>
        void Emit(IGameEvent e);
    }
}
