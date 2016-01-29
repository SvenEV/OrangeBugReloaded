using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Rendering;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// A pin that pops <see cref="BalloonEntity"/> of the same color as the pin.
    /// Balloons that do not match the color of the pin cannot move onto the <see cref="PinTile"/>.
    /// </summary>
    [VisualHint("PinTile-{Color}")]
    public class PinTile : Tile
    {
        /// <summary>
        /// The pin color.
        /// A pin only pops balloons that match its color.
        /// </summary>
        public InkColor Color { get; }

        public PinTile(InkColor color)
        {
            Color = color;
        }

        internal override async Task AttachEntityAsync(TileEventArgs e)
        {
            if (e.Transaction.CurrentMove.Entity is PlayerEntity)
            {
                // Players may walk over pins, use default behavior
                await base.AttachEntityAsync(e);
            }
            else if ((e.Transaction.CurrentMove.Entity as BalloonEntity)?.Color == Color)
            {
                // Balloons are only allowed if the color matches.
                // In this case they are destroyed.
                if (Entity != Entity.None)
                    await Entity.DetachAsync(e, this);

                if (!e.Transaction.IsCancelled)
                {
                    e.Result = this;
                    e.Transaction.Emit(new BalloonPopEvent(Color));
                }
            }
            else
            {
                // All other entities are rejected
                e.Transaction.Cancel();
            }
        }
    }

    public class BalloonPopEvent : IGameEvent, IAudioHint
    {
        /// <summary>
        /// The color of the <see cref="BalloonEntity"/> that got popped.
        /// </summary>
        public InkColor Color { get; }

        public string AudioKey => "BalloonEntity-Popped";

        public BalloonPopEvent(InkColor color)
        {
            Color = color;
        }
    }
}
