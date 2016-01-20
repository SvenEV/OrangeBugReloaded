using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Tiles;
using System;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Describes a move of an <see cref="Entity"/> from one
    /// <see cref="Tile"/> (or null) to another (or null).
    /// </summary>
    public class EntityMoveInfo
    {
        /// <summary>
        /// The entity that is moved.
        /// </summary>
        public Entity Entity { get; }

        /// <summary>
        /// Indicates why the entity is being moved.
        /// </summary>
        public MoveReason Reason { get; }

        /// <summary>
        /// The <see cref="ILocation"/> the <see cref="Entity"/> is moved away (detached) from.
        /// May be null if the Entity is not attached to anything yet.
        /// </summary>
        public ILocation Source { get; }

        /// <summary>
        /// The <see cref="ILocation"/> the <see cref="Entity"/> is moved (attached) to.
        /// May not be null.
        /// </summary>
        public ILocation Target { get; }

        /// <summary>
        /// The distance between <see cref="Source"/> and <see cref="Target"/>.
        /// Is <see cref="Point.Zero"/> if <see cref="Source"/> is null.
        /// </summary>
        public Point Offset { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMoveInfo"/> class.
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="source">Source</param>
        /// <param name="target">Target</param>
        public EntityMoveInfo(Entity entity, ILocation source, ILocation target)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Determine reason
            if (source == null)
            {
                if (target == null)
                    throw new InvalidOperationException($"Parameters {nameof(source)} and {nameof(target)} may not be both null");
                else
                    Reason = MoveReason.InitialPlacement;
            }
            else
            {
                if (target == null)
                    Reason = MoveReason.DestructionMove;
                else
                    Reason = MoveReason.Move;
            }

            Entity = entity;
            Source = source;
            Target = target;

            Offset = (source == null || target == null) ?
                Point.Zero :
                target.Position - source.Position;
        }

        /// <summary>
        /// Describes the reason or function of an <see cref="Entity"/> move.
        /// </summary>
        public enum MoveReason
        {
            /// <summary>
            /// The <see cref="Entity"/> is being moved from its current <see cref="Tile"/>
            /// to another <see cref="Tile"/>.
            /// </summary>
            Move,

            /// <summary>
            /// The <see cref="Entity"/> is being moved from null to a <see cref="Tile"/>.
            /// This indicates that a new entity is being moved onto the <see cref="IMap"/>
            /// for the first time.
            /// </summary>
            InitialPlacement,

            /// <summary>
            /// The <see cref="Entity"/> is being moved from its current <see cref="Tile"/>
            /// to null. This indicates that the entity is leaving the <see cref="IMap"/>
            /// in order to be destroyed.
            /// </summary>
            DestructionMove            
        }
    }
}
