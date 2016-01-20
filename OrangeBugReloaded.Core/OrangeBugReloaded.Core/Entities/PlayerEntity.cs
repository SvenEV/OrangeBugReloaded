using System.Threading.Tasks;
using Newtonsoft.Json;
using OrangeBugReloaded.Core.Tiles;
using OrangeBugReloaded.Core.Designing;

namespace OrangeBugReloaded.Core.Entities
{
    /// <summary>
    /// This is Orange Bug!
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [GameObjectMetadata(Name = "Player")]
    public class PlayerEntity : Entity
    {
        [JsonProperty(PropertyName = nameof(IsInfected))]
        private bool _isInfected = false;

        [JsonProperty(PropertyName = nameof(Perspective))]
        private Point _perspective = Point.East;

        /// <summary>
        /// Indicates whether Orange Bug is infected.
        /// <seealso cref="PathTile.CausesInfection"/>
        /// </summary>
        public bool IsInfected
        {
            get { return _isInfected; }
            set { Set(ref _isInfected, value); }
        }

        /// <summary>
        /// The direction the player is looking towards.
        /// </summary>
        public Point Perspective
        {
            get { return _perspective; }
            set { Set(ref _perspective, value); }
        }

        /// <inheritdoc/>
        internal override Task<bool> TryDetachAsync(EntityMoveContext e) => False; // Players can't be forced to leave a tile

        /// <summary>
        /// Sets the <see cref="Perspective"/> according to
        /// the movement direction (regardless of the movement result).
        /// </summary>
        /// <param name="e"></param>
        internal override void OnBeginMove(EntityMoveContext e)
        {
            if (e.Initiator == this && e.CurrentMove.Offset.IsDirection)
                Perspective = e.CurrentMove.Offset;
        }
    }
}
