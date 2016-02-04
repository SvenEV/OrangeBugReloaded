using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Rendering;
using System;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// A tile that can extend in one of the four directions
    /// based on the state of an <see cref="ITrigger"/>.
    /// When the piston extends it pushes the entity next to it
    /// one tile towards the extension direction.
    /// </summary>
    public class PistonTile : Tile
    {
        // This defines a dependy 2 tiles towards the piston direction.
        // Example scenario: [Piston][Box][Player] where the piston direction is East
        // an the trigger is on. Now if the player moves away we want the piston to extend.
        // NOTE: In the future if we want the piston to have the force to push multiple
        //       entities we somehow need multiple relative dependencies
        //       e.g. 2 * Direction, 3 * Direction, 4 * Direction, ...
        [MapDependency(IsRelative = true)]
        private Point _neighborDependency => Direction + Direction;

        [MapDependency]
        public Point TriggerPosition { get; }

        /// <summary>
        /// The direction in which the piston extends.
        /// </summary>
        /// <remarks>
        /// This is a relative map dependency so that the following scenario is
        /// correctly handled: [Piston][Box][Balloon] where the piston direction
        /// is East and the trigger is on. Now if the box is pushed away we want
        /// the piston to extend.
        /// </remarks>
        [MapDependency(IsRelative = true)]
        public Point Direction { get; }

        /// <summary>
        /// Indicates whether the piston is in the extended state.
        /// </summary>
        public bool IsExtended { get; }

        public PistonTile(Point triggerPosition, Point direction, bool isExtended = false)
            : base(isExtended ? Entity.None : new PistonEntity())
        {
            TriggerPosition = triggerPosition;
            Direction = direction.EnsureDirection();
            IsExtended = isExtended;
        }

        internal override Task AttachEntityAsync(AttachEventArgs e)
        {
            if (IsExtended && e.CurrentMove.Entity is PistonEntity)
            {
                e.Result = new PistonTile(TriggerPosition, Direction, false);
            }
            else
            {
                e.Cancel();
            }

            return Task.CompletedTask;
        }

        internal override Task DetachEntityAsync(TileEventArgs e)
        {
            if (!IsExtended && e.CurrentMove.Entity is PistonEntity)
            {
                e.Result = new PistonTile(TriggerPosition, Direction, true);
            }
            else
            {
                throw new InvalidOperationException("The piston has an invalid entity on it");
            }

            return Task.CompletedTask;
        }

        internal override async Task OnFollowUpTransactionAsync(FollowUpEventArgs e, Point position)
        {
            var trigger = await e.GetAsync(TriggerPosition) as ITrigger;
            var isTriggerOn = trigger?.IsOn ?? false;

            if (!IsExtended && isTriggerOn)
            {
                // Extend piston
                if (await e.MoveAsync(position, position + Direction))
                    e.Emit(new PistonExtendRetractEvent(true));
            }
            else if (IsExtended && !isTriggerOn)
            {
                // Retract piston
                if (await e.MoveAsync(position + Direction, position))
                    e.Emit(new PistonExtendRetractEvent(false));
            }

            // Otherwise do nothing
        }
    }

    public class PistonExtendRetractEvent : IGameEvent, IAudioHint
    {
        public bool IsExtended { get; }

        public string AudioKey => $"PistonTile-{(IsExtended ? "Extended" : "Retracted")}";

        public PistonExtendRetractEvent(bool isExtended)
        {
            IsExtended = isExtended;
        }
    }
}
