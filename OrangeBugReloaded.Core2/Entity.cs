using OrangeBugReloaded.Core.Entities;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    public abstract class Entity : IEquatable<Entity>
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
        public virtual Task BeginMoveAsync(EntityBeginMoveArgs e)
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
        /// <returns>Task</returns>
        public virtual Task DetachAsync(EntityDetachArgs e)
        {
            // Default behavior: Cancel transaction, entity cannot be pushed or collected
            e.Cancel();
            return Task.CompletedTask;
        }

        /// <summary>
        /// A helper method that implements behavior which makes the entity
        /// be pushed to a neighbor tile if the pushing entity is a <see cref="PlayerEntity"/>.
        /// Can be used in <see cref="DetachAsync(TileEventArgs, Tile)"/>.
        /// </summary>
        /// <param name="e">Event arguments</param>
        /// <returns>Task</returns>
        protected static async Task DetachByPushingAsync(EntityDetachArgs e)
        {
            if (e.CurrentMove.Entity is IPusher && e.SuggestedPushDirection.IsDirection)
            {
                await e.MoveAsync(
                    e.CurrentMove.TargetPosition,
                    e.CurrentMove.TargetPosition + e.SuggestedPushDirection);
            }
            else
            {
                e.Cancel();
            }
        }

        /// <summary>
        /// A helper method that implements behavior which makes the entity
        /// be collectable by a <see cref="PlayerEntity"/>.
        /// Can be used in <see cref="DetachAsync(TileEventArgs, Tile)"/>.
        /// </summary>
        /// <param name="e">Event arguments</param>
        /// <returns>Task</returns>
        protected static Task DetachByCollectingAsync(EntityDetachArgs e)
        {
            if (e.CurrentMove.Entity is PlayerEntity)
            {
                // Player collects the entity
                e.Result = Tile.WithoutEntity(e.Tile);
            }
            else
            {
                e.Cancel();
            }

            return Task.CompletedTask;
        }
        
        /// <inheritdoc/>
        public override string ToString() => GetType().Name;

        public override bool Equals(object obj) => Equals(obj as Entity);

        public bool Equals(Entity other)
        {
            if (GetType() != other?.GetType())
                return false;

            var selfProps = GetHashProperties().Cast<object>();
            var otherProps = other.GetHashProperties().Cast<object>();

            return selfProps.Zip(otherProps, Equals).All(b => b);
        }

        public override sealed int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 17;
                hash = hash * 23 + GetType().GetHashCode();

                foreach (var prop in GetHashProperties())
                    hash = hash * 23 + prop.GetHashCode();

                return hash;
            }
        }

        protected virtual IEnumerable GetHashProperties() { yield break; }
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
            {
                Debugger.Break();
                throw new ArgumentException($"Entity is null in {callerMemberName}");
            }

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
            {
                Debugger.Break();
                throw new ArgumentException($"Entity is null or none in {callerMemberName}");
            }
            return entity;
        }
    }
}
