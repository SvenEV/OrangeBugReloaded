using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Foundation;
using OrangeBugReloaded.Core.Tiles;
using System;
using System.Collections.Generic;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Provides context to a chain of entity moves.
    /// For example, if a <see cref="PlayerEntity"/> pushes a
    /// <see cref="BoxEntity"/> in order to move, this class
    /// provides the three tiles that are involved and a
    /// <see cref="LockContext"/> to lock
    /// the three tiles and both entities.
    /// </summary>
    public class EntityMoveContext : IDisposable
    {
        /// <summary>
        /// The <see cref="IMap"/>.
        /// </summary>
        public IMap Map { get; }

        /// <summary>
        /// Indicates whether the context is disposed. If true, no new entity moves
        /// can be executed using this context; a new context has to be used instead.
        /// This prevents recycling of the context passed to
        /// <see cref="Tile.OnEntityMovesCompletedAsync(EntityMoveContext)"/>.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// The object that has caused the chain of moves.
        /// Can be manipulated during multiple moves
        /// in the same context, see
        /// <see cref="TeleporterTile"/> for example.
        /// </summary>
        public OrangeBugGameObject Initiator { get; set; }

        /// <summary>
        /// The current recursion depth (length of the move chain).
        /// </summary>
        public int CurrentRecursionDepth { get; internal set; } = 0;

        /// <summary>
        /// Provides information about which entity
        /// currently moves from where to where.
        /// </summary>
        public EntityMoveInfo CurrentMove { get; internal set; }

        /// <summary>
        /// A set of locations that are involved in the chain of moves.
        /// </summary>
        public HashSet<Point> AffectedLocations { get; } = new HashSet<Point>();

        /// <summary>
        /// A <see cref="LockContext"/> that is
        /// used to lock <see cref="Entity"/> and <see cref="Tile"/>
        /// instances during the movement chain.
        /// </summary>
        public LockContext Locking { get; }

        /// <summary>
        /// Initializes a new <see cref="EntityMoveContext"/>.
        /// </summary>
        /// <param name="map">The <see cref="IMap"/></param>
        /// <param name="initiator">The initiator of the move chain</param>
        public EntityMoveContext(IMap map, OrangeBugGameObject initiator)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            Map = map;
            Initiator = initiator;

            // TODO: "LocalPlayer" should be replaced with the name of the
            // currently logged in user somehow
            Locking = LockContext.Create<MapLocation>(loc => loc.TileLock);//, "LocalPlayer");
        }

        /// <summary>
        /// Disposes the <see cref="EntityMoveContext"/> so that it cannot
        /// be used to invoke a new entity move.
        /// </summary>
        public void Dispose()
        {
            CurrentMove = null;
            IsDisposed = true;
        }
    }
}
