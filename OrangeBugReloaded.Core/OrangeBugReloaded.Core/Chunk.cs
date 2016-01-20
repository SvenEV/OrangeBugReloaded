using Newtonsoft.Json;
using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Tiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Groups together a square of <see cref="MapLocation"/> instances.
    /// <seealso cref="IMap"/>
    /// </summary> 
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Chunk : IEnumerable<MapLocation>
    {
        /// <summary>
        /// The uniform width and length of a <see cref="Chunk"/> in tiles.
        /// </summary>
        public const int Size = 8;

        [JsonProperty(PropertyName = "Locations", Order = 1)]
        private MapLocation[,] _locations;

        /// <summary>
        /// Determines the position of the <see cref="Chunk"/> on the map.
        /// </summary>
        [JsonProperty(Order = 0)]
        public Point Index { get; private set; }

        /// <summary>
        /// Indicates whether the <see cref="Chunk"/> has changed since
        /// it was last saved. Chunks are considered changed
        /// if a <see cref="Tile"/> or <see cref="Entity"/> has been added,
        /// changed or removed in any way.
        /// Internal changes in tiles or entities,
        /// such as <see cref="PlayerEntity.Perspective"/>, do not cause this
        /// property to be set to true (TODO: Is this intended?).
        /// </summary>
        public bool HasChanged { get; internal set; }

        /// <summary>
        /// Indicates whether the <see cref="Chunk"/> is empty
        /// which means that there are no tiles.
        /// </summary>
        public bool IsEmpty => _locations.Cast<MapLocation>().All(o => o.IsEmpty);

        /// <summary>
        /// Initializes a new empty <see cref="Chunk"/> with the specified index.
        /// </summary>
        /// <param name="index">Chunk index</param>
        public Chunk(Point index)
        {
            Index = index;
            _locations = new MapLocation[Size, Size];
            Repair(); // Initialize _locations with new Location-objects
            HasChanged = true;
        }

        /// <summary>
        /// Recreates the <see cref="Entity.Owner"/> references of
        /// <see cref="Entity"/> instances and the
        /// <see cref="Tile.Location"/> references of <see cref="Tile"/>
        /// instances.
        /// Also subscribes to property changes that "invalidate" the chunk
        /// (<see cref="HasChanged"/>).
        /// </summary>
        public void Repair()
        {
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var location = _locations[x, y];

                    if (location == null)
                        location = _locations[x, y] = new MapLocation();

                    location.Position = Index * Size + new Point(x, y);
                    location.Subscribe(nameof(MapLocation.Tile), OnSomethingInLocationHasChanged);

                    if (location.Tile != null)
                    {
                        location.Tile.Location = location;
                        location.Tile.Subscribe(nameof(Tile.Entity), OnSomethingInLocationHasChanged);

                        if (location.Tile.Entity != null)
                            location.Tile.Entity.Owner = location.Tile;
                    }

                    if (location.TileTemplate != null)
                    {
                        location.TileTemplate.Location = location;
                        location.TileTemplate.Subscribe(nameof(Tile.Entity), OnSomethingInLocationHasChanged);

                        if (location.TileTemplate.Entity != null)
                            location.TileTemplate.Entity.Owner = location.TileTemplate;
                    }

                }
            }

            HasChanged = false;
        }

        /// <summary>
        /// Gets the <see cref="MapLocation"/> at the specified position.
        /// </summary>
        /// <param name="p">Position in range [0..Size]</param>
        /// <returns>Tile</returns>
        public MapLocation this[Point p]
        {
            get
            {
                if (p.X < 0 || p.X >= Size || p.Y < 0 || p.Y >= Size)
                    throw new IndexOutOfRangeException(p.ToString());

                return _locations[p.X, p.Y];
            }
        }

        /// <summary>
        /// Gets the <see cref="MapLocation"/> at the specified position.
        /// </summary>
        /// <param name="x">Position X [0..Size]</param>
        /// <param name="y">Position Y [0..Size]</param>
        /// <returns>MapLocation</returns>
        public MapLocation this[int x, int y] => this[new Point(x, y)];
        
        /// <summary>
        /// Creates a copy of the <see cref="Chunk"/>.
        /// Every <see cref="Tile"/> is cloned too.
        /// </summary>
        /// <returns></returns>
        internal Chunk DeepClone()
        {
            var chunkCopy = (Chunk)MemberwiseClone();
            chunkCopy._locations = new MapLocation[Size, Size];

            for (var y = 0; y < Size; y++)
                for (var x = 0; x < Size; x++)
                    chunkCopy._locations[x, y] = _locations[x, y]?.DeepClone();

            return chunkCopy;
        }

        private void OnSomethingInLocationHasChanged()
        {
            // If an entity has been added, moved or destroyed the chunk has changed
            // and we need to save it during the next save procedure.

            if (!HasChanged)
            {
                Logger.LogInfo($"Chunk {Index} has invalidated");
                HasChanged = true;
            }
        }

        /// <inheritdoc/>
        public IEnumerator<MapLocation> GetEnumerator() => _locations.OfType<MapLocation>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
