using Newtonsoft.Json;
using OrangeBugReloaded.Core.Tiles;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    public abstract class Tile : GameObject, IEquatable<Tile>
    {
        public static Tile Empty { get; } = WallTile.Default;

        [JsonProperty]
        public Entity Entity { get; private set; } = Entity.None;

        /// <summary>
        /// The delay between updates of the tile and
        /// its follow up transactions.
        /// </summary>
        [JsonIgnore]
        public virtual TimeSpan FollowUpDelay => TimeSpan.FromSeconds(1);

        public Tile()
        {
        }

        protected Tile(Entity entity)
        {
            Entity = entity.EnsureNotNull();
        }

        /// <summary>
        /// Attaches an <see cref="Entity"/> to the <see cref="Tile"/>.
        /// </summary>
        /// <remarks>
        /// This method typically implements at least one of the following behaviors:
        /// * Accept the entity: return a composition of the tile and the entity
        /// * If the tile is already occupied, detach the current entity
        /// * Cancel the move
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
        internal virtual async Task AttachEntityAsync(IAttachArgs e)
        {
            if (Entity != Entity.None)
            {
                var detachArgs = e.CreateEntityDetachArgs(this, e.CurrentMove.Offset.IsDirection ? e.CurrentMove.Offset : Point.Zero);
                await Entity.DetachAsync(detachArgs);
            }

            if (!e.IsSealed)
            {
                e.Result = Compose(this, e.CurrentMove.Entity);
            }
        }

        /// <summary>
        /// Detaches an <see cref="Entity"/> from the <see cref="Tile"/>.
        /// </summary>
        /// <remarks>
        /// This method typically implements at least on of the following behaviors:
        /// * Remove the entity: return the tile without the entity attached
        /// * Cancel the move
        /// 
        /// Default behavior:
        /// The entity is correctly detached from the tile.
        /// No properties of the tile are modified.
        /// </remarks>
        /// <param name="e">Event arguments</param>
        /// <returns>Task</returns>
        internal virtual Task DetachEntityAsync(IDetachArgs e)
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
        /// (initiate further moves using
        /// <see cref="OnFollowUpTransactionAsync(FollowUpEventArgs, Point)"/> instead).
        /// </remarks>
        /// <param name="e">Event arguments</param>
        /// <returns>Task</returns>
        internal virtual Task OnEntityMoveTransactionCompletedAsync(IMovesCompletedArgs e)
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
        internal virtual Task OnFollowUpTransactionAsync(IFollowUpArgs e, Point position) => Task.CompletedTask;

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
            // QUESTION: Should we cache created instances?
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
            // QUESTION: Should we cache created instances?
            var newTile = (T)tile.EnsureNotNull().MemberwiseClone();
            newTile.Entity = Entity.None;
            return newTile;
        }

        public override bool Equals(object obj) => Equals(obj as Tile);

        public bool Equals(Tile other)
        {
            if (GetType() != other?.GetType())
                return false;

            var selfProps = GetHashProperties().Cast<object>();
            var otherProps = other.GetHashProperties().Cast<object>();

            return
                selfProps.Zip(otherProps, Equals).All(b => b) &&
                Equals(Entity, other.Entity);
        }

        public override sealed int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 17;
                hash = hash * 23 + GetType().GetHashCode();

                foreach (var prop in GetHashProperties())
                    hash = hash * 23 + prop.GetHashCode();

                if (Entity != Entity.None)
                    hash = hash * 23 + Entity.GetHashCode();

                return hash;
            }
        }

        protected virtual IEnumerable GetHashProperties() { yield break; }

        public override string ToString() =>
            VisualKey +
            (Entity != Entity.None ? " + " + Entity.VisualKey : "");

        public static bool operator ==(Tile a, Tile b) => Equals(a, b);
        public static bool operator !=(Tile a, Tile b) => !Equals(a, b);
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
            {
                Debugger.Break();
                throw new ArgumentException($"Tile is null in '{callerMemberName}'");
            }

            return tile;
        }
    }
}
