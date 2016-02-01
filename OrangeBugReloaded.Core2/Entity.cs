using OrangeBugReloaded.Core.Entities;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    public abstract class Entity
    {
        public static Entity None { get; } = NullEntity.Default;

        /// <summary>
        /// Is called before a movement is executed
        /// (even if the movement will fail).
        /// </summary>
        /// <remarks>
        /// This method allows the following actions:
        /// * Change the entity before it is moved
        /// * Cancel the transaction
        /// * Initiate another move
        /// </remarks>
        /// <param name="e">Event arguments</param>
        public virtual Task BeginMoveAsync(EntityEventArgs e)
        {
            e.Result = this;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Is called in order to make the entity detach itself from its tile.
        /// </summary>
        /// <remarks>
        /// Typically the desired behavior is achieved by
        /// * Making the entity pushable using the helper method <see cref="DetachByPushingAsync(TileEventArgs, Tile)"/>
        /// * Making the entity collectable using the helper method <see cref="DetachByCollectingAsync(TileEventArgs, Tile)"/>
        /// </remarks>
        /// <param name="e">Event arguments</param>
        /// <param name="tile">The tile</param>
        /// <returns>Task</returns>
        public virtual Task DetachAsync(TileEventArgs e, Tile tile)
        {
            // Default behavior: Cancel transaction, entity cannot be pushed or collected
            e.Transaction.Cancel();
            return Task.CompletedTask;
        }

        /// <summary>
        /// A helper method that implements behavior which makes the entity
        /// be pushed to a neighbor tile if the pushing entity is a <see cref="PlayerEntity"/>.
        /// Can be used in <see cref="DetachAsync(TileEventArgs, Tile)"/>.
        /// </summary>
        /// <param name="e">Event arguments</param>
        /// <param name="tile">The tile</param>
        /// <returns>Task</returns>
        protected static async Task DetachByPushingAsync(TileEventArgs e, Tile tile)
        {
            if (e.Transaction.CurrentMove.Entity is IPusher && e.Transaction.CurrentMove.Offset.IsDirection)
            {
                await e.Transaction.MoveAsync(
                    e.Transaction.CurrentMove.TargetPosition,
                    e.Transaction.CurrentMove.TargetPosition + e.Transaction.CurrentMove.Offset);
            }
            else
            {
                e.Transaction.Cancel();
            }
        }

        /// <summary>
        /// A helper method that implements behavior which makes the entity
        /// be collectable by a <see cref="PlayerEntity"/>.
        /// Can be used in <see cref="DetachAsync(TileEventArgs, Tile)"/>.
        /// </summary>
        /// <param name="e">Event arguments</param>
        /// <param name="tile">The tile</param>
        /// <returns>Task</returns>
        protected static Task DetachByCollectingAsync(TileEventArgs e, Tile tile)
        {
            if (e.Transaction.CurrentMove.Entity is PlayerEntity)
            {
                // Player collects the entity
                e.Result = Tile.WithoutEntity(tile);
            }
            else
            {
                e.Transaction.Cancel();
            }

            return Task.CompletedTask;
        }
        
        /// <inheritdoc/>
        public override string ToString() => GetType().Name;
    }

    public static class EntityExtensions
    {
        /// <summary>
        /// Throws an exception if the specified <see cref="Entity"/> is null.
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="callerMemberName">Caller member name (automatically inserted)</param>
        /// <returns>The entity</returns>
        /// <exception cref="ArgumentException">If the entity is null</exception>
        public static Entity EnsureNotNull(this Entity entity, [CallerMemberName]string callerMemberName = null)
        {
            if (entity == null)
                throw new ArgumentException($"Entity is null in {callerMemberName}");
            return entity;
        }

        /// <summary>
        /// Throws an exception if the specified <see cref="Entity"/> is null
        /// or <see cref="Entity.None"/>.
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="callerMemberName">Caller member name (automatically inserted)</param>
        /// <returns>The entity</returns>
        /// <exception cref="ArgumentException">If the entity is null or <see cref="Entity.None"/></exception>
        public static Entity EnsureNotNone(this Entity entity, [CallerMemberName]string callerMemberName = null)
        {
            if (entity.EnsureNotNull() == Entity.None)
                throw new ArgumentException($"Entity is null or none in {callerMemberName}");
            return entity;
        }
    }
}
