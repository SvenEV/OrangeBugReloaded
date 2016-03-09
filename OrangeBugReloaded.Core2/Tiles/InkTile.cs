using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Presentation;
using System.Threading.Tasks;
using System.Collections;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// Ink that applies its color to a <see cref="BalloonEntity"/>.
    /// Ink can only be used once. When the ink is consumed the tile
    /// transforms into a <see cref="PathTile"/>.
    /// </summary>
    public class InkTile : Tile
    {
        /// <summary>
        /// The ink color.
        /// </summary>
        public InkColor Color { get; }

        public override string VisualKey => $"InkTile-{Color}";

        public InkTile(InkColor color)
        {
            Color = color;
        }

        internal override Task OnEntityMoveTransactionCompletedAsync(IMovesCompletedArgs e)
        {
            var balloon = Entity as BalloonEntity;

            if (balloon != null)
            {
                // Consume the ink to color the balloon
                e.Result = Compose(PathTile.Default, new BalloonEntity(Color));

                if (Color != balloon.Color)
                    e.Emit(new InkTileConsumeEvent(balloon.Color, Color));
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
