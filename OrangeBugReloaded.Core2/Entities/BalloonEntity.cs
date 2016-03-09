using OrangeBugReloaded.Core.Presentation;
using System.Threading.Tasks;
using System.Collections;

namespace OrangeBugReloaded.Core.Entities
{
    /// <summary>
    /// A balloon that can be pushed around,
    /// recolored by an <see cref="Tiles.InkTile"/> and
    /// destroyed by a <see cref="Tiles.PinTile"/>.
    /// </summary>
    public class BalloonEntity : Entity
    {
        /// <summary>
        /// The balloon color.
        /// </summary>
        public InkColor Color { get; }

        public override string VisualKey => $"BalloonEntity-{Color}";

        public BalloonEntity(InkColor color)
        {
            Color = color;
        }

        public override Task DetachAsync(IEntityDetachArgs e)
            => DetachByPushingAsync(e);

        protected override IEnumerable GetHashProperties()
        {
            yield return Color;
        }
    }
}
