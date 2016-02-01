using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Rendering;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// Ink that applies its color to a <see cref="BalloonEntity"/>.
    /// Ink can only be used once. When the ink is consumed the tile
    /// transforms into a <see cref="PathTile"/>.
    /// </summary>
    [VisualHint("InkTile-{Color}")]
    public class InkTile : Tile
    {
        /// <summary>
        /// The ink color.
        /// </summary>
        public InkColor Color { get; }

        public InkTile(InkColor color)
        {
            Color = color;
        }

        internal override async Task AttachEntityAsync(AttachEventArgs e)
        {
            if (e.Transaction.CurrentMove.Entity is BalloonEntity)
            {
                // Balloons are colored according to the InkTile's color
                if (Entity != Entity.None)
                    await Entity.DetachAsync(e, this);

                if (!e.Transaction.IsCancelled)
                {
                    e.Result = Compose(PathTile.Default, new BalloonEntity(Color));
                    var oldColor = ((BalloonEntity)e.Transaction.CurrentMove.Entity).Color;
                    if (Color != oldColor) e.Transaction.Emit(new InkTileConsumeEvent(oldColor, Color));
                }
            }
            else
            {
                // All other entities may move over the InkTile, use default behavior
                await base.AttachEntityAsync(e);
            }
        }
    }

    public class InkTileConsumeEvent : IGameEvent, IAudioHint
    {
        /// <summary>
        /// The color of the <see cref="BalloonEntity"/> before it got
        /// colored by the <see cref="InkTile"/>.
        /// </summary>
        public InkColor BalloonColor { get; }

        /// <summary>
        /// The color of the <see cref="InkTile"/>.
        /// </summary>
        public InkColor InkTileColor { get; }

        public string AudioKey => "InkTile-Consumed";

        public InkTileConsumeEvent(InkColor balloonColor, InkColor inkTileColor)
        {
            BalloonColor = balloonColor;
            InkTileColor = inkTileColor;
        }
    }
}
