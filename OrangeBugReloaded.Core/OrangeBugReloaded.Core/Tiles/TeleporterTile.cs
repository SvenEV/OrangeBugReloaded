using Newtonsoft.Json;
using OrangeBugReloaded.Core.Designing;
using OrangeBugReloaded.Core.Entities;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// A <see cref="Tile"/> which magically transports a <see cref="PlayerEntity"/> to another position.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [GameObjectMetadata(Name = "Teleporter", Description = "Magically transports a player to another position")]
    public class TeleporterTile : Tile
    {
        [JsonProperty(PropertyName = "TargetPosition")]
        private Point _targetPosition;

        [JsonProperty(PropertyName = "AcceptedEntities")]
        private EntityFilterMode _acceptedEntities;

        /// <summary>
        /// The target of the teleportation.
        /// </summary>
        [Editable("Target Position")]
        public Point TargetPosition
        {
            get { return _targetPosition; }
            set { Set(ref _targetPosition, value); }
        }

        /// <summary>
        /// The <see cref="Entity"/> subset that this
        /// <see cref="TeleporterTile"/> can teleport.
        /// </summary>
        [Editable("Accepted Entities")]
        public EntityFilterMode AcceptedEntities
        {
            get { return _acceptedEntities; }
            set { Set(ref _acceptedEntities, value); }
        }

        /// <inheritdoc/>
        protected override async Task OnEntityMovesCompletedAsync(EntityMoveContext e)
        {
            if (Entity != null && AcceptedEntities.Includes(Entity.GetType()) && !(e.Initiator is TeleporterTile))
            {
                // Now the initiator is the teleporter itself, so
                // if the target is a teleporter as well the entity
                // won't end up in an endless loop of teleportations.
                // (We must use a new context, 'e' is already disposed here)
                var context = new EntityMoveContext(e.Map, this);
                await Entity.TryMoveAsync(TargetPosition, context);
            }
        }
    }
}
