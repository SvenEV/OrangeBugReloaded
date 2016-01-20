using Newtonsoft.Json;
using OrangeBugReloaded.Core.Tiles;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Entities
{
    /// <summary>
    /// Describes a thing that can be attached and detached to/from
    /// <see cref="Tile"/> instances. 
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public abstract class Entity : OrangeBugGameObject
    {
        private Tile _owner;

        /// <summary>
        /// The <see cref="Tile"/> the entity is attached to.
        /// </summary>
        public Tile Owner
        {
            get { return _owner; }
            internal set { Set(ref _owner, value); }
        }

        /// <summary>
        /// Tries to move the <see cref="Entity"/> towards
        /// the specified direction. If <paramref name="direction"/>
        /// is not a valid direction nothing happens.
        /// </summary>
        /// <param name="direction">Direction</param>
        /// <param name="context">An existing EntityMovementContext (if null, a new one is created)</param>
        /// <returns>True if the movement has been successful</returns>
        public async Task<bool> TryMoveTowardsAsync(Point direction, EntityMoveContext context = null)
        {
            if (!direction.IsDirection)
                return false; // 'direction' is not a valid direction

            if (_owner == null)
            {
                Logger.LogError($"Failed to move '{this}': Entity must be attached to a tile in order to move it towards a direction");
                return false;
            }

            if (!(_owner.Location is MapLocation))
            {
                Logger.LogError($"{nameof(TryMoveTowardsAsync)} is only supported on {nameof(MapLocation)}s.");
                return false;
            }

            var map = ((MapLocation)_owner.Location).Map;
            context = context ?? new EntityMoveContext(map, this);

            return await TryMoveAsync(_owner.Location.Position + direction, context);
        }

        /// <summary>
        /// Tries to move the <see cref="Entity"/> to the
        /// specified position.
        /// </summary>
        /// <param name="targetPosition">Target position</param>
        /// <param name="context">Movement context</param>
        /// <param name="forceDetach">
        /// If true detachment from the <see cref="Entity"/>'s owner tile is enforced
        /// and <see cref="Tile.CanDetachEntityAsync(EntityMoveContext)"/> is not evaluated.
        /// </param>
        /// <returns>True if the movement has been successful</returns>
        public async Task<bool> TryMoveAsync(Point? targetPosition, EntityMoveContext context, bool forceDetach = false)
        {
            // This public method is only responsible for cleaning up after a
            // chain of moves is completed, i.e. when recursion depth is 0.

            if (context.IsDisposed)
                throw new ObjectDisposedException(nameof(EntityMoveContext));

            var oldMoveInfo = context.CurrentMove;
            context.CurrentRecursionDepth++;
            var result = await TryMoveCoreAsync(targetPosition, context, forceDetach);
            context.CurrentRecursionDepth--;
            context.CurrentMove = oldMoveInfo;

            if (context.CurrentRecursionDepth == 0 && result)
            {
                // Dispose the context so that it cannot be used to execute
                // further entity moves as these would modify 'AffectedLocations'
                // while we are enumerating it below.
                context.Dispose();

                // Notify all tiles that have been affected by the move
                // (and further recursive moves) that their entity might have changed.
                // See GateTile or ButtonTile for implementation examples.
                var affectedTilesTasks = context.AffectedLocations
                    .Select(async p =>
                    {
                        var location = await context.Map.TryGetAsync(p);
                        var tile = location?.Tile;
                        if (tile != null) await tile.RaiseEntityMovesCompletedAsync(context);
                    });

                await TaskEx.WhenAll(affectedTilesTasks);
            }

            return result;
        }

        private async Task<bool> TryMoveCoreAsync(Point? targetPosition, EntityMoveContext context, bool forceDetach)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            /* Steps involved in an entity move:
             1) Lock the SourceTile
             2) Get & Lock the TargetTile
             3) Add SourceTile & TargetTile to AffectedLocations
             4) Entity.OnBeginMove
             5) Check SourceTile.CanDetach?
             6) Check TargetTile.CanAttach?
             7) SourceTile.EntityDetached
             8) Owner = TargetTile
             9) TargetTile.EntityAttached
            10) Entity.OnMoved
            */

            using (var padlockSource = context.Locking.Lock(_owner?.Location as MapLocation))
            {
                if (!padlockSource.IsValid)
                    return false;

                var targetLocation = targetPosition.HasValue ? await context.Map.TryGetAsync(targetPosition.Value) : null;
                var args = new EntityMoveInfo(this, _owner?.Location, targetLocation);
                context.CurrentMove = args;

                using (var padlockTarget = context.Locking.Lock(targetLocation))
                {
                    // If the entity, the source tile or the target
                    // tile is already in use, cancel the movement.
                    if (!padlockTarget.IsValid)
                        return false;

                    if (_owner != null)
                        context.AffectedLocations.Add(_owner.Location.Position);

                    if (targetPosition.HasValue)
                        context.AffectedLocations.Add(targetPosition.Value);

                    OnBeginMove(context);

                    var targetOwner = targetLocation?.Tile;

                    // Check if there is a tile at the target position
                    if (targetPosition != null && targetOwner == null)
                    {
                        Logger.Log($"Failed to move '{this}': There is no tile at {targetPosition} (source was '{_owner?.ToString() ?? "NULL"}')");
                        return false;
                    }

                    if (_owner == targetOwner)
                        return true;

                    // Check if we can detach the entity from its owner
                    if (_owner != null && !(forceDetach || await _owner.CanDetachEntityAsync(context)))
                    {
                        Logger.Log($"Failed to move '{this}': Could not detach from '{_owner}' (target was '{targetOwner?.ToString() ?? "NULL"}')");
                        return false;
                    }

                    // Check if we can attach the entity to the new owner
                    if (targetOwner != null && !await targetOwner.CanAttachEntityAsync(context))
                    {
                        Logger.Log($"Failed to move '{this}': Could not attach to '{targetOwner}' (source was '{_owner?.ToString() ?? "NULL"}')");
                        return false;
                    }

                    // Raise EntityDetached
                    if (_owner != null)
                    {
                        _owner.Entity = null;
                        await _owner.RaiseEntityDetachedAsync(context);
                    }

                    // Update Owner property
                    var oldOwner = _owner;
                    Owner = targetOwner;

                    // Raise EntityAttached
                    if (targetOwner != null)
                    {
                        targetOwner.Entity = this;
                        await targetOwner.RaiseEntityAttachedAsync(context);
                    }

                    Logger.Log($"Entity '{this}' moved from '{oldOwner?.ToString() ?? "NULL"}' to '{targetOwner?.ToString() ?? "NULL"}'");
                    OnMoved(context);

                    return true;
                }
            }
        }

        /// <summary>
        /// Tries to detach the <see cref="Entity"/> from its <see cref="Owner"/>
        /// tile. If the target tile of an entity move is occupied by
        /// another entity, this method is called on the occupying entity in order
        /// to clear the target tile in favor of the moving/calling entity.
        /// Implementations could, for example, destroy the entity
        /// (for collectables) or move the entity in the same direction as
        /// the calling entity (for things like crates that can be pushed).
        /// </summary>
        /// <param name="e">Context</param>
        /// <returns>True if the entity could be detached; false otherwise</returns>
        internal abstract Task<bool> TryDetachAsync(EntityMoveContext e);

        /// <summary>
        /// Is called before a movement is executed
        /// (even if the movement will fail).
        /// </summary>
        /// <param name="e">Event arguments</param>
        internal virtual void OnBeginMove(EntityMoveContext e) { }

        /// <summary>
        /// Is called after a successful move.
        /// </summary>
        /// <param name="e">Event arguments</param>
        internal virtual void OnMoved(EntityMoveContext e) { }

        /// <summary>
        /// Creates a shallow copy of the <see cref="Entity"/>.
        /// <see cref="Owner"/> and PropertyChanged handlers
        /// are set to their default values on the clone.
        /// </summary>
        /// <returns>Entity clone</returns>
        internal Entity Clone()
        {
            var clone = (Entity)CloneWithoutHandlers(this);
            clone._owner = null;
            return clone;
        }

        /// <inheritdoc/>
        public override string ToString() => GetType().Name;
    }
}
