using OrangeBugReloaded.Core.Rendering;
using System;
using System.Threading.Tasks;
using System.Collections;

namespace OrangeBugReloaded.Core.Entities
{
    /// <summary>
    /// This is Orange Bug!
    /// </summary>
    public class PlayerEntity : Entity, IPusher
    {
        public Point Perspective { get; }

        public string PlayerId { get; }

        public override Point VisualOrientation => Perspective;

        public PlayerEntity(string playerId, Point perspective)
        {
            if (!perspective.IsDirection)
                throw new ArgumentException("Invalid direction", nameof(perspective));

            PlayerId = playerId;
            Perspective = perspective;
        }

        public override Task BeginMoveAsync(IBeginMoveArgs e)
        {
            var offset = e.CurrentMove.Offset;

            if (offset == Point.North)
                e.ResultingEntity = new PlayerEntity(PlayerId, Point.North);
            else if (offset == Point.East)
                e.ResultingEntity = new PlayerEntity(PlayerId, Point.East);
            else if (offset == Point.South)
                e.ResultingEntity = new PlayerEntity(PlayerId, Point.South);
            else if (offset == Point.West)
                e.ResultingEntity = new PlayerEntity(PlayerId, Point.West);
            else
                e.ResultingEntity = this;

            return Task.CompletedTask;
        }

        public override Task DetachAsync(IEntityDetachArgs e)
            => DetachByPushingAsync(e);

        protected override IEnumerable GetHashProperties()
        {
            yield return Perspective;
            yield return PlayerId;
        }
    }
}
