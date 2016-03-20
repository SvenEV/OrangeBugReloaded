using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;

namespace OrangeBugReloaded.Core.Tiles
{
    public class CornerTile : Tile
    {
        public Point Orientation { get; }

        public override Point VisualOrientation => Orientation;
        public CornerTile(Point orientation)
        {
            Orientation = orientation;
        }

        internal override async Task AttachEntityAsync(IAttachArgs e)
        {
            var inDirection = e.CurrentMove.Offset;
            var outDirection = _outDirections.TryGetValue(new Tuple<Point, Point>(Orientation, inDirection));

            if (outDirection.IsDirection)
            {
                if (Entity != Entity.None)
                    await Entity.DetachAsync(e.CreateEntityDetachArgs(this, outDirection));

                if (!e.IsSealed)
                    e.Result = Compose(this, e.CurrentMove.Entity);
            }
            else
            {
                // The entity tries to move across a wall side of the corner
                e.Seal();
            }
        }

        internal override Task DetachEntityAsync(IDetachArgs e)
        {
            if (e.CurrentMove.Offset.IsDirection && !GetOutDirections().Contains(e.CurrentMove.Offset))
            {
                // The entity tries to leave the tile across a wall side of the corner
                e.Seal();
            }
            else
            {
                e.Result = WithoutEntity(this);
            }

            return Task.CompletedTask;
        }

        private IEnumerable<Point> GetOutDirections()
        {
            yield return Orientation;
            yield return new Point(-Orientation.Y, Orientation.X);
        }

        private IEnumerable<Point> GetInDirections()
        {
            yield return -Orientation;
            yield return new Point(Orientation.Y, -Orientation.X);
        }

        private static readonly Dictionary<Tuple<Point, Point>, Point> _outDirections = new Dictionary<Tuple<Point, Point>, Point>
        {
            { new Tuple<Point, Point>(Point.North, Point.South), Point.West },
            { new Tuple<Point, Point>(Point.North, Point.East), Point.North },
            { new Tuple<Point, Point>(Point.East, Point.West), Point.North },
            { new Tuple<Point, Point>(Point.East, Point.South), Point.East },
            { new Tuple<Point, Point>(Point.South, Point.North), Point.East },
            { new Tuple<Point, Point>(Point.South, Point.West), Point.South },
            { new Tuple<Point, Point>(Point.West, Point.East), Point.South },
            { new Tuple<Point, Point>(Point.West, Point.North), Point.West }
        };

        protected override IEnumerable GetHashProperties()
        {
            yield return Orientation;
        }
    }
}
