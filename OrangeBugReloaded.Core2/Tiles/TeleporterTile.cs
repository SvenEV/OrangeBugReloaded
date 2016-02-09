using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Rendering;
using System.Threading.Tasks;
using System.Collections;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// A device that magically transports entities to another location.
    /// </summary>
    public class TeleporterTile : Tile
    {
        /// <summary>
        /// The position where entities are transported to.
        /// </summary>
        public Point TargetPosition { get; }

        /// <summary>
        /// The <see cref="Entity"/> subset that this
        /// <see cref="TeleporterTile"/> can teleport.
        /// </summary>
        public EntityFilterMode AcceptedEntities { get; }

        public TeleporterTile(Point targetPosition, EntityFilterMode acceptedEntities = EntityFilterMode.EntitiesExceptPlayer)
        {
            TargetPosition = targetPosition;
            AcceptedEntities = acceptedEntities;
        }

        internal override async Task OnFollowUpTransactionAsync(FollowUpEventArgs e, Point position)
        {
            // Teleport using a new transaction
            // (because if teleportation fails, we still want every other move
            // that happened before to apply)

            if (Entity != Entity.None && AcceptedEntities.Includes(Entity.GetType()) &&
                !(e.Initiator is TeleporterTile))
            {

                // Now the initiator is the teleporter itself, so
                // if the target is a teleporter as well the entity
                // won't end up in an endless loop of teleportations.
                e.Initiator = this;

                var isSuccessful = await e.MoveAsync(position, TargetPosition);

                // TODO: Events triggered within MoveAsync are emitted before the teleport event
                if (isSuccessful)
                    e.Emit(new TeleporterTileTeleportEvent(position, TargetPosition, Entity));
            }
        }

        protected override IEnumerable GetHashProperties()
        {
            yield return TargetPosition;
            yield return AcceptedEntities;
        }
    }

    public class TeleporterTileTeleportEvent : IGameEvent, IAudioHint
    {
        /// <summary>
        /// The position of the teleporter.
        /// </summary>
        public Point SourcePosition { get; }

        /// <summary>
        /// The position where the entity has been transported to.
        /// </summary>
        public Point TargetPosition { get; }

        /// <summary>
        /// The entity that has been teleported.
        /// </summary>
        public Entity Entity { get; }

        public string AudioKey => "TeleporterTile-Teleported";

        public TeleporterTileTeleportEvent(Point sourcePosition, Point targetPosition, Entity entity)
        {
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
            Entity = entity;
        }
    }
}
