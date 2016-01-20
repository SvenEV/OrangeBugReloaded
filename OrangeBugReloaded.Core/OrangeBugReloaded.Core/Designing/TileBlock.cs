using OrangeBugReloaded.Core.Foundation;
using OrangeBugReloaded.Core.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace OrangeBugReloaded.Core.Designing
{
    /// <summary>
    /// An arbitrarily shaped area of <see cref="Tile"/>s.
    /// </summary>
    public class TileBlock : BindableBase, ILocationsProvider, IEnumerable<TileBlockLocation>
    {
        private Rectangle _bounds;

        /// <summary>
        /// Determines the size of the <see cref="TileBlock"/>.
        /// </summary>
        public Rectangle Bounds
        {
            get { return _bounds; }
            private set { Set(ref _bounds, value); }
        }

        /// <summary>
        /// The tiles (and their positions) included in the <see cref="TileBlock"/>.
        /// </summary>
        public ObservableDictionary<Point, ILocation> Locations { get; }

        /// <summary>
        /// Initializes a new <see cref="TileBlock"/> with the
        /// specified <see cref="MapLocation"/>s.
        /// For each location a copy of the tile template is created.
        /// </summary>
        /// <param name="locationsOnMap"></param>
        public TileBlock(MapLocation[] locationsOnMap)
        {
            // Create a copy of each TileTemplate and wrap each
            // copy in a TileBlockLocation
            var tileBlockLocations = locationsOnMap.ToDictionary(o => o.Position, o =>
            {
                var clone = o.TileTemplate.DeepClone();
                clone.Location = new TileBlockLocation(o.Position, clone);
                return clone.Location;
            });

            Locations = new ObservableDictionary<Point, ILocation>(tileBlockLocations);

            if (locationsOnMap.Any())
            {
                _bounds = Rectangle.FromEdges(
                    locationsOnMap.Select(o => o.Position.X).Min(),
                    locationsOnMap.Select(o => o.Position.Y).Max(),
                    locationsOnMap.Select(o => o.Position.X).Max(),
                    locationsOnMap.Select(o => o.Position.Y).Min());
            }
            else
            {
                _bounds = Rectangle.Zero;
            }
        }

        /// <summary>
        /// Gets a location of the TileBlock.
        /// </summary>
        /// <param name="p">Position</param>
        /// <returns>
        /// <see cref="TileBlockLocation"/> or null if the <see cref="TileBlock"/> does not
        /// include a <see cref="TileBlockLocation"/> at the specified position.
        /// </returns>
        public TileBlockLocation this[Point p]
        {
            get
            {
                ILocation location;
                return Locations.TryGetValue(p, out location) ? (TileBlockLocation)location : null;
            }
        }

        /// <inheritdoc/>
        public IEnumerator<TileBlockLocation> GetEnumerator()
            => Locations.Values.Cast<TileBlockLocation>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
