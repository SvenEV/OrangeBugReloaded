using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Rendering;
using System;
using System.Threading.Tasks;
using System.Collections;
using OrangeBugReloaded.Core.Transactions;

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

        public override Point VisualOrientation => Direction;

        public PistonTile(Point triggerPosition, Point direction, bool isExtended = false)
            : base(isExtended ? Entity.None : new PistonEntity(direction))
        {
            TriggerPosition = triggerPosition;
            Direction = direction.EnsureDirection();
            IsExtended = isExtended;
        }

        internal override Task AttachEntityAsync(IAttachArgs e)
        {
            if (IsExtended && e.CurrentMove.Entity is PistonEntity)
            {
                e.Result = new PistonTile(TriggerPosition, Direction, false);
            }
            else
            {
                e.StopRecording();
            }

            return Task.CompletedTask;
        }

        internal override Task DetachEntityAsync(IDetachArgs e)
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

        internal override async Task OnFollowUpTransactionAsync(IFollowUpArgs e, Point position)
        {
            if (e.Initiator.Object is PistonTile)
                return;

            var trigger = (await e.GetAsync(TriggerPosition)).Tile as ITrigger;
            var isTriggerOn = trigger?.IsOn ?? false;

            var initiator = new MoveInitiator(this, position);

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

        protected override IEnumerable GetHashProperties()
        {
            yield return TriggerPosition;
            yield return IsExtended;
            yield return Direction;
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
