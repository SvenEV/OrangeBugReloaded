using OrangeBugReloaded.Core.Foundation;
using OrangeBugReloaded.Core.Tiles;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Definese properties and methods an <see cref="ILocation"/>
    /// implementation must support.
    /// </summary>
    public interface ILocation : IBindable
    {
        /// <summary>
        /// The position of the <see cref="ILocation"/>.
        /// </summary>
        Point Position { get; }

        /// <summary>
        /// The tile that is assigned to the <see cref="ILocation"/>.
        /// </summary>
        Tile Tile { get; }
    }
}
