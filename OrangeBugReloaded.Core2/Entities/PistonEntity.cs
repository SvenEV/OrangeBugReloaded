using System.Collections;
using OrangeBugReloaded.Core.Rendering;

namespace OrangeBugReloaded.Core.Entities
{
    /// <summary>
    /// The entity that represents the extension
    /// of a <see cref="Tiles.PistonTile"/>.
    /// </summary>
    [VisualHint("PistonEntity", nameof(Direction))]
    public class PistonEntity : Entity, IPusher
    {
        /// <summary>
        /// The direction in which the piston extends.
        /// </summary>
        [MapDependency(IsRelative = true)]
        public Point Direction { get; }

        public PistonEntity(Point direction)
        {
            Direction = direction;
        }

        protected override IEnumerable GetHashProperties()
        {
            yield return Direction;
        }
    }
}
