using System;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Entities
{
    /// <summary>
    /// This is Orange Bug!
    /// </summary>
    public class PlayerEntity : Entity
    {
        public static PlayerEntity PlayerLookingNorth { get; } = new PlayerEntity(Point.North);
        public static PlayerEntity PlayerLookingEast { get; } = new PlayerEntity(Point.East);
        public static PlayerEntity PlayerLookingSouth { get; } = new PlayerEntity(Point.South);
        public static PlayerEntity PlayerLookingWest { get; } = new PlayerEntity(Point.West);

        public Point Perspective { get; }

        private PlayerEntity(Point perspective)
        {
            if (!perspective.IsDirection)
                throw new ArgumentException("Invalid direction", nameof(perspective));

            Perspective = perspective;
        }

        public override Task BeginMoveAsync(EntityEventArgs e)
        {
            var offset = e.Transaction.CurrentMove.Offset;

            if (offset == Point.North)
                e.Result = PlayerLookingNorth;
            else if (offset == Point.East)
                e.Result = PlayerLookingEast;
            else if (offset == Point.South)
                e.Result = PlayerLookingSouth;
            else if (offset == Point.West)
                e.Result = PlayerLookingWest;
            else
                e.Result = this;

            return Task.CompletedTask;
        }
    }
}
