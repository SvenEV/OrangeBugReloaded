using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrangeBugReloaded.Core.Designing;
using OrangeBugReloaded.Core.Entities;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// Destroys <see cref="BalloonEntity"/> instances.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [GameObjectMetadata(Name = "Pin")]
    public class PinTile : Tile
    {
        [JsonProperty(PropertyName = "Color")]
        [JsonConverter(typeof(StringEnumConverter))]
        private InkColor _color;

        /// <summary>
        /// Color. Only <see cref="BalloonEntity"/> instances
        /// of the same color can be destroyed.
        /// </summary>
        [Editable]
        public InkColor Color
        {
            get { return _color; }
            set { Set(ref _color, value); }
        }

        internal override async Task<bool> CanAttachEntityAsync(EntityMoveContext e) =>
            (e.CurrentMove.Entity is PlayerEntity || (e.CurrentMove.Entity as BalloonEntity)?.Color == Color) &&
            await base.CanAttachEntityAsync(e);

        /// <inheritdoc/>
        protected override async Task OnEntityAttachedAsync(EntityMoveContext e)
        {
            var balloon = Entity as BalloonEntity;

            if (balloon != null && balloon.Color == Color)
            {
                // Cast is safe because entity movement is only possible on Maps
                await ((MapLocation)Location).Map.DestroyEntityAsync(Location.Position, e);
            }
        }
    }
}
