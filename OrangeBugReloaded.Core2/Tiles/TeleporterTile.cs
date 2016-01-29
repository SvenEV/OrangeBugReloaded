using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Rendering;
using System.Threading.Tasks;

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
            if (Entity != Entity.None && AcceptedEntities.Includes(Entity.GetType()) &&
                !(e.CompletedTransaction.Initiator is TeleporterTile))
            {
                // Create a new transaction for teleportation
                // (because if teleportation fails, we still want every other move
                // that happened before to apply)
                var teleportTransaction = e.CreateFollowUpTransaction();

                // Now the initiator is the teleporter itself, so
                // if the target is a teleporter as well the entity
                // won't end up in an endless loop of teleportations.
                teleportTransaction.Initiator = this;
                var isSuccessful = await teleportTransaction.MoveAsync(position, TargetPosition);

                // TODO: Events triggered within MoveAsync are emitted before the teleport event
                if (isSuccessful)
                    e.CompletedTransaction.Emit(new TeleporterTileTeleportEvent(position, TargetPosition, Entity));
            }
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
