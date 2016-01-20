using Newtonsoft.Json;
using OrangeBugReloaded.Core.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// The objects an <see cref="IMap"/> is made of.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public abstract class Tile : OrangeBugGameObject
    {
        private Task _activationTask;
        private Task _deactivationTask;

        [JsonProperty(PropertyName = "Entity", NullValueHandling = NullValueHandling.Ignore)]
        private Entity _entity;

        /// <summary>
        /// Provides information about the position of the <see cref="Tile"/>.
        /// </summary>
        public ILocation Location { get; internal set; }

        /// <summary>
        /// The <see cref="Entity"/> that is attached to the <see cref="Tile"/>.
        /// </summary>
        public Entity Entity
        {
            get { return _entity; }
            internal set { Set(ref _entity, value); }
        }

        /// <summary>
        /// Event handler for entity move events.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Context</param>
        /// <returns>Task</returns>
        public delegate Task EntityMoveEventHandler(Tile sender, EntityMoveContext e);

        /// <summary>
        /// Is called after a whole recursive chain of entity moves has
        /// succeeded and the <see cref="Tile"/> has executed its
        /// <see cref="OnEntityMovesCompletedAsync"/>-logic.
        /// </summary>
        public event EntityMoveEventHandler EntityMovesCompleted;

        /// <summary>
        /// Is called when an <see cref="Entity"/> will be attached.
        /// Event handlers are executed before the <see cref="Tile"/>
        /// executes its <see cref="OnEntityAttachedAsync(EntityMoveContext)"/>-logic.
        /// </summary>
        public event EntityMoveEventHandler EntityAttaching;

        /// <summary>
        /// Is called when an <see cref="Entity"/> will be detached.
        /// Event handlers are executed before the <see cref="Tile"/>
        /// executes its <see cref="OnEntityDetachedAsync(EntityMoveContext)"/>-logic.
        /// </summary>
        public event EntityMoveEventHandler EntityDetaching;

        internal async Task RaiseEntityMovesCompletedAsync(EntityMoveContext e)
        {
            try
            {
                // Yes, the order "OnEntityMovesCompletedAsync first, then the event handlers" matters, do not change
                await OnEntityMovesCompletedAsync(e);

                if (EntityMovesCompleted != null)
                {
                    var handlers = EntityMovesCompleted.GetInvocationList().Select(o => ((EntityMoveEventHandler)o)(this, e));
                    await TaskEx.WhenAll(handlers);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        internal async Task RaiseEntityAttachedAsync(EntityMoveContext e)
        {
            try
            {
                // Yes, the order "EntityAttaching-event first, then OnEntityAttachedAsync" matters, do not change

                if (EntityAttaching != null)
                {
                    var handlers = EntityAttaching.GetInvocationList().Select(o => ((EntityMoveEventHandler)o)(this, e));
                    await TaskEx.WhenAll(handlers);
                }

                await OnEntityAttachedAsync(e);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        internal async Task RaiseEntityDetachedAsync(EntityMoveContext e)
        {
            try
            {
                // Yes, the order "EntityDetaching-event first, then OnEntityDetachedAsync" matters, do not change

                if (EntityDetaching != null)
                {
                    var handlers = EntityDetaching.GetInvocationList().Select(o => ((EntityMoveEventHandler)o)(this, e));
                    await TaskEx.WhenAll(handlers);
                }

                await OnEntityDetachedAsync(e);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }


        /// <summary>
        /// Determines whether an <see cref="Entity"/> can be attached to the
        /// <see cref="Tile"/>.
        /// Default behavior: An entity of any type can be attached if the tile
        /// is empty (not occupied) or if the occupying entity can be detached.
        /// Simply put: Either the tile is empty or whatever is on it must go away!
        /// </summary>
        /// <param name="e">Context</param>
        /// <returns>True if the entity can be attached, false otherwise</returns>
        internal virtual async Task<bool> CanAttachEntityAsync(EntityMoveContext e)
            => Entity == null || await Entity.TryDetachAsync(e);

        /// <summary>
        /// Determines whether an <see cref="Entity"/> can be detached from the
        /// <see cref="Tile"/>. Default behavior: Every entity can be detached.
        /// </summary>
        /// <param name="e">Context</param>
        /// <returns>True if the entity can be detached, false otherwise</returns>
        internal virtual Task<bool> CanDetachEntityAsync(EntityMoveContext e)
            => True;


        /// <summary>
        /// Is called after a whole recursive chain of entity moves has
        /// succeeded. Use this if your tile needs to check the final
        /// entity status after all chained moves have completed.
        /// Note that the specified <see cref="EntityMoveContext"/> may not
        /// be used to execute further entity moves; create a new context instead.
        /// </summary>
        protected virtual Task OnEntityMovesCompletedAsync(EntityMoveContext e) => Done;

        /// <summary>
        /// Is called when an entity has been attached to the tile
        /// during a movement process.
        /// </summary>
        protected virtual Task OnEntityAttachedAsync(EntityMoveContext e) => Done;

        /// <summary>
        /// Is called when an entity has been detached from the tile
        /// during a movement process.
        /// </summary>
        protected virtual Task OnEntityDetachedAsync(EntityMoveContext e) => Done;


        internal async Task ActivateAsync()
        {
            if (!(Location is MapLocation))
                throw new InvalidOperationException($"Activation is only allowed in conjunction with {nameof(MapLocation)}.");

            if (_deactivationTask != null)
                await _deactivationTask; // First, wait until deactivated (if deactivation is in progress)

            if (_activationTask == null)
                _activationTask = ActivateCoreAsync();

            await _activationTask;
        }

        internal async Task DeactivateAsync()
        {
            if (!(Location is MapLocation))
                throw new InvalidOperationException($"Deactivation is only allowed in conjunction with {nameof(MapLocation)}.");

            if (_activationTask != null)
                await _activationTask; // First, wait until activated (if activation is in progress)

            if (_deactivationTask == null)
                _deactivationTask = DeactivateCoreAsync();

            await _deactivationTask;
        }


        private async Task ActivateCoreAsync()
        {
            var mapLocation = Location as MapLocation;

            if (mapLocation== null)
                throw new InvalidOperationException($"Cannot activate {nameof(Tile)} without a valid {nameof(MapLocation)}");

            // TODO: Anything else to do here?
            // Any events to call on Entity itself?

            // If there's an entity on this tile fake an
            // InitialPlacement-move
            // HACK/TODO: Do we need this?
            if (Entity != null)
            {
                var context = new EntityMoveContext(mapLocation.Map, this);
                context.CurrentMove = new EntityMoveInfo(Entity, null, Location);
                await RaiseEntityAttachedAsync(context);
                await RaiseEntityMovesCompletedAsync(context);
            }

            // TODO: Also call OnEntityMovesCompleted (for GateTile)

            await OnActivateAsync();
        }

        private async Task DeactivateCoreAsync()
        {
            // TODO: Anything else to do here?
            await OnDeactivateAsync();
        }


        /// <summary>
        /// Is called after the tile has been attached to a Map.
        /// </summary>
        protected virtual Task OnActivateAsync() => Done;

        /// <summary>
        /// Is called after the tile has been detached from a Map.
        /// At this point Map, Position and Entity have been reset and
        /// all PropertyChanged subscriptions to this tile have been cleared.
        /// </summary>
        protected virtual Task OnDeactivateAsync() => Done;



        /// <summary>
        /// Creates a copy of the <see cref="Tile"/> which is shallow
        /// with one exception: The <see cref="Entity"/> is also cloned.
        /// Event handlers and <see cref="Location"/> are removed
        /// on the clone.
        /// </summary>
        /// <returns>Tile clone</returns>
        internal Tile DeepClone()
        {
            var tileCopy = (Tile)CloneWithoutHandlers(this);
            tileCopy.EntityMovesCompleted = null;
            tileCopy.EntityAttaching = null;
            tileCopy.EntityDetaching = null;
            tileCopy.Location = null;

            if (Entity != null)
            {
                var entityCopy = Entity.Clone();
                entityCopy.Owner = tileCopy;
                tileCopy.Entity = entityCopy;
            }

            return tileCopy;

        }

        /// <inheritdoc/>
        public override string ToString() => GetType().Name;
    }
}
