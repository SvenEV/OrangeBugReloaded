using OrangeBugReloaded.Core.Entities;
using System;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Tiles
{
    public class PistonTile : Tile
    {
        [MapDependency]
        public Point TriggerPosition { get; }

        public Point Direction { get; }

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
            if (IsExtended && e.Transaction.CurrentMove.Entity is PistonEntity)
            {
                e.Result = new PistonTile(TriggerPosition, Direction, false);
            }
            else
            {
                e.Transaction.Cancel();
            }

            return Task.CompletedTask;
        }

        internal override Task DetachEntityAsync(TileEventArgs e)
        {
            if (!IsExtended && e.Transaction.CurrentMove.Entity is PistonEntity)
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
            var trigger = await e.CompletedTransaction.GetAsync(TriggerPosition) as ITrigger;
            var isTriggerOn = trigger?.IsOn ?? false;

            if (!IsExtended && isTriggerOn)
            {
                // Extend piston
                var t = e.CreateFollowUpTransaction();
                await t.MoveAsync(position, position + Direction);
            }
            else if (IsExtended && !isTriggerOn)
            {
                // Retract piston
                var t = e.CreateFollowUpTransaction();
                await t.MoveAsync(position + Direction, position);
            }

            // Otherwise do nothing
        }
    }
}
