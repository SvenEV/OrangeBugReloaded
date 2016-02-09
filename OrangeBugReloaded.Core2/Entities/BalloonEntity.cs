using OrangeBugReloaded.Core.Rendering;
using System.Threading.Tasks;
using System.Collections;

namespace OrangeBugReloaded.Core.Entities
{
    /// <summary>
    /// A balloon that can be pushed around,
    /// recolored by an <see cref="Tiles.InkTile"/> and
    /// destroyed by a <see cref="Tiles.PinTile"/>.
    /// </summary>
    [VisualHint("BalloonEntity-{Color}")]
    public class BalloonEntity : Entity
    {
        /// <summary>
        /// The balloon color.
        /// </summary>
        public InkColor Color { get; }

        public BalloonEntity(InkColor color)
        {
            Color = color;
        }

        public override Task DetachAsync(TileEventArgs e, Tile tile)
            => DetachByPushingAsync(e, tile);

        protected override IEnumerable GetHashProperties()
        {
            yield return Color;
        }
    }
}
