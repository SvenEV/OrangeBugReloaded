using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrangeBugReloaded.Core.Designing;
using OrangeBugReloaded.Core.Tiles;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Entities
{
    /// <summary>
    /// A balloon that can be pushed around,
    /// colored by <see cref="InkTile"/>
    /// and destroyed by <see cref="PinTile"/>.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [GameObjectMetadata(Name = "Balloon")]
    public class BalloonEntity : Entity
    {
        [JsonProperty(PropertyName = "Color")]
        [JsonConverter(typeof(StringEnumConverter))]
        private InkColor _color;

        /// <summary>
        /// Balloon color.
        /// </summary>
        [Editable]
        public InkColor Color
        {
            get { return _color; }
            set { Set(ref _color, value); }
        }

        /// <inheritdoc/>
        internal override async Task<bool> TryDetachAsync(EntityMoveContext e) =>
            e.CurrentMove.Entity is PlayerEntity && e.Initiator is PlayerEntity && await TryMoveTowardsAsync(e.CurrentMove.Offset, e); // This enables balloons to be pushed
    }
}
