using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Rendering;
using System.Threading.Tasks;
using System.Collections;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// A pin that pops <see cref="BalloonEntity"/> of the same color as the pin.
    /// Balloons that do not match the color of the pin cannot move onto the <see cref="PinTile"/>.
    /// </summary>
    public class PinTile : Tile
    {
        /// <summary>
        /// The pin color.
        /// A pin only pops balloons that match its color.
        /// </summary>
        public InkColor Color { get; }

        public override string VisualKey => $"PinTile-{Color}";

        public PinTile(InkColor color)
        {
            Color = color;
        }

        internal override async Task AttachEntityAsync(IAttachArgs e)
        {
            if (e.CurrentMove.Entity is PlayerEntity)
            {
                // Players may walk over pins, use default behavior
                await base.AttachEntityAsync(e);
            }
            else if ((e.CurrentMove.Entity as BalloonEntity)?.Color == Color)
            {
                // Balloons that match the pin's color are allowed
                e.Result = Compose(this, e.CurrentMove.Entity);
            }
            else
            {
                // All other entities are rejected
                e.StopRecording();
            }
        }

        internal override Task OnEntityMoveTransactionCompletedAsync(IMovesCompletedArgs e)
        {
            if ((Entity as BalloonEntity)?.Color == Color)
            {
                // Destroy the balloon
                e.Result = new PinTile(Color);
                e.Emit(new BalloonPopEvent(Color));
            }
            else
            {
                // Otherwise, nothing changes
                e.Result = this;
            }

            return Task.CompletedTask;
        }

        protected override IEnumerable GetHashProperties()
        {
            yield return Color;
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
