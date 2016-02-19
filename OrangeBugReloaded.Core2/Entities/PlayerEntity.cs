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

        public override Task BeginMoveAsync(IBeginMoveArgs e)
        {
            var offset = e.CurrentMove.Offset;

            if (offset == Point.North)
                e.ResultingEntity = new PlayerEntity(Id, Point.North);
            else if (offset == Point.East)
                e.ResultingEntity = new PlayerEntity(Id, Point.East);
            else if (offset == Point.South)
                e.ResultingEntity = new PlayerEntity(Id, Point.South);
            else if (offset == Point.West)
                e.ResultingEntity = new PlayerEntity(Id, Point.West);
            else
                e.ResultingEntity = this;

            return Task.CompletedTask;
        }

        public override Task DetachAsync(IEntityDetachArgs e)
            => DetachByPushingAsync(e);

        protected override IEnumerable GetHashProperties()
        {
            yield return Perspective;
            yield return Id;
        }
    }
}
