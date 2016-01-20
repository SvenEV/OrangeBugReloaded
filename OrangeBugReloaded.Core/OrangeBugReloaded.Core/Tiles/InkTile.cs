using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrangeBugReloaded.Core.Designing;
using OrangeBugReloaded.Core.Entities;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// Applies a color to a <see cref="BalloonEntity"/>.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [GameObjectMetadata(Name = "Ink")]
    public class InkTile : Tile
    {
        [JsonProperty(PropertyName = "Color")]
        [JsonConverter(typeof(StringEnumConverter))]
        private InkColor _color;

        [JsonProperty(PropertyName = "IsUsed")]
        private bool _isUsed;

        /// <summary>
        /// Color to apply to a <see cref="BalloonEntity"/>.
        /// </summary>
        [Editable]
        public InkColor Color
        {
            get { return _color; }
            set { Set(ref _color, value); }
        }

        /// <summary>
        /// Indicates whether the ink has already been used
        /// (ink can only be consumed once).
        /// </summary>
        public bool IsUsed
        {
            get { return _isUsed; }
            set { Set(ref _isUsed, value); }
        }

        /// <inheritdoc/>
        protected override Task OnEntityAttachedAsync(EntityMoveContext e)
        {
            var balloon = Entity as BalloonEntity;
            
            if (balloon != null && !_isUsed)
            {
                balloon.Color = Color;
                IsUsed = true;
            }

            return Done;
        }
    }
}
