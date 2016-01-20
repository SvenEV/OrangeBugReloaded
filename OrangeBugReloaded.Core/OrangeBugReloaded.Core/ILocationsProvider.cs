using OrangeBugReloaded.Core.Foundation;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Provides an observable collection of <see cref="ILocation"/>s
    /// to be observed and visualized by the UI or graphics engine.
    /// </summary>
    public interface ILocationsProvider
    {
        /// <summary>
        /// A collection of all <see cref="ILocation"/> instances.
        /// </summary>
        ObservableDictionary<Point, ILocation> Locations { get; }
    }
}
