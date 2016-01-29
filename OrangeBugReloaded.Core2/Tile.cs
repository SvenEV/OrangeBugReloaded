﻿using OrangeBugReloaded.Core.Tiles;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    public abstract class Tile
    {
        public static Tile Empty { get; } = WallTile.Default;

        public Entity Entity { get; private set; } = Entity.None;

        /// <summary>
        /// Attaches an <see cref="Entity"/> to the <see cref="Tile"/>.
        /// </summary>
        /// <remarks>
        /// This method typically implements at least one of the following behaviors:
        /// * Accept the entity: return a composition of the tile and the entity
        /// * If the tile is already occupied, detach the current entity
        /// * Cancel the transaction
        /// * Initiate another move
        /// 
        /// Default behavior:
        /// If the tile is already occupied by an entity, the entity is asked to detach.
        /// (see <see cref="Entity.DetachAsync(TileEventArgs, Tile)"/>).
        /// If the detachment is successful or the tile is not occupied,
        /// the new entity is correctly attached to the tile.
        /// No properties of the tile or the entity are modified.
        /// </remarks>
        /// <param name="e">Event arguments</param>
        /// <returns>Task</returns>
        internal virtual async Task AttachEntityAsync(TileEventArgs e)
        {
            if (Entity != Entity.None)
                await Entity.DetachAsync(e, this);

            if (!e.Transaction.IsCancelled)
            {
                e.Result = Compose(this, e.Transaction.CurrentMove.Entity);
            }
        }

        /// <summary>
        /// Detaches an <see cref="Entity"/> from the <see cref="Tile"/>.
        /// </summary>
        /// <remarks>
        /// This method typically implements at least on of the following behaviors:
        /// * Remove the entity: return the tile without the entity attached
        /// * Cancel the transaction
        /// 
        /// Default behavior:
        /// The entity is correctly detached from the tile.
        /// No properties of the tile are modified.
        /// </remarks>
        /// <param name="e">Event arguments</param>
        /// <returns>Task</returns>
        internal virtual Task DetachEntityAsync(TileEventArgs e)
        {
            // Default behavior: Entities are correctly detached from the tile
            e.Result = WithoutEntity(this);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Is called immediately after an entity move transaction has finished.
        /// The method is called on all tiles that were involved in the transaction
        /// or are directly or transitively dependent on any of those tiles.
        /// </summary>
        /// <remarks>
        /// Use the method to check map dependencies and update the tile accordingly.
        /// Implementers of this method must not cancel the transaction.
        /// Implementers of this method must not initiate further moves
        /// (initiate further moves using
        /// <see cref="OnFollowUpTransactionAsync(FollowUpEventArgs, Point)"/> instead).
        /// </remarks>
        /// <param name="e">Event arguments</param>
        /// <returns>Task</returns>
        internal virtual Task OnEntityMoveTransactionCompletedAsync(TileEventArgs e)
        {
            e.Result = this;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Is called immediately after <see cref="OnEntityMoveTransactionCompletedAsync(TileEventArgs)"/>
        /// to spawn further transactions as a consequence of the completed transaction.
        /// The method is called on all tiles that were involved in the transaction
        /// or are directly or transitively dependent on any of those tiles.
        /// </summary>
        /// <param name="e">Event arguments</param>
        /// <param name="position">The position of the tile</param>
        /// <returns>Task</returns>
        internal virtual Task OnFollowUpTransactionAsync(FollowUpEventArgs e, Point position) => Task.CompletedTask;

        /// <summary>
        /// Combines the specified <see cref="Tile"/> and the specified <see cref="Entity"/>
        /// into a new tile.
        /// </summary>
        /// <typeparam name="T">Tile type</typeparam>
        /// <param name="tile">Tile</param>
        /// <param name="entity">Entity</param>
        /// <returns>Tile with the entity attached</returns>
        public static T Compose<T>(T tile, Entity entity) where T : Tile
        {
            // TODO: Cache created instances
            var newTile = (T)tile.EnsureNotNull().MemberwiseClone();
            newTile.Entity = entity.EnsureNotNull();
            return newTile;
        }

        /// <summary>
        /// Returns a new <see cref="Tile"/> that equals the specified tile
        /// but has no <see cref="Entity"/> attached.
        /// </summary>
        /// <typeparam name="T">Tile type</typeparam>
        /// <param name="tile">Tile</param>
        /// <returns>Tile without an entity attached</returns>
        public static T WithoutEntity<T>(T tile) where T : Tile
        {
            // TODO: Cache created instances
            var newTile = (T)tile.EnsureNotNull().MemberwiseClone();
            newTile.Entity = Entity.None;
            return newTile;
        }
    }

    public static class TileExtensions
    {
        /// <summary>
        /// Throws an exception if the specified <see cref="Tile"/> is null.
        /// </summary>
        /// <param name="tile">Tile</param>
        /// <param name="callerMemberName">Caller member name (automatically inserted)</param>
        /// <returns>The tile</returns>
        /// <exception cref="ArgumentException">If the tile is null</exception>
        public static Tile EnsureNotNull(this Tile tile, [CallerMemberName]string callerMemberName = null)
        {
            if (tile == null)
                throw new ArgumentException($"Tile is null in '{callerMemberName}'");
            return tile;
        }
    }
}
