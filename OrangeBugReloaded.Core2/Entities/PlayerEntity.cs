using OrangeBugReloaded.Core.Rendering;
using System;
using System.Threading.Tasks;
using System.Collections;

namespace OrangeBugReloaded.Core.Entities
{
    /// <summary>
    /// This is Orange Bug!
    /// </summary>
    [VisualHint("PlayerEntity", nameof(Perspective))]
    public class PlayerEntity : Entity, IPusher
    {
        public Point Perspective { get; }

        public string Id { get; }

        public PlayerEntity(string playerId, Point perspective)
        {
            if (!perspective.IsDirection)
                throw new ArgumentException("Invalid direction", nameof(perspective));

            Id = playerId;
            Perspective = perspective;
        }

        public override Task BeginMoveAsync(EntityEventArgs e)
        {
            var offset = e.CurrentMove.Offset;

            if (offset == Point.North)
                e.Result = new PlayerEntity(Id, Point.North);
            else if (offset == Point.East)
                e.Result = new PlayerEntity(Id, Point.East);
            else if (offset == Point.South)
                e.Result = new PlayerEntity(Id, Point.South);
            else if (offset == Point.West)
                e.Result = new PlayerEntity(Id, Point.West);
            else
                e.Result = this;

            return Task.CompletedTask;
        }

        protected override IEnumerable GetHashProperties()
        {
            yield return Perspective;
            yield return Id;
        }
    }
}
