using Newtonsoft.Json;
using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Foundation;
using OrangeBugReloaded.Core.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Represents a location on an <see cref="IMap"/>.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MapLocation : BindableBase, ILocation
    {
        private static readonly Random _random = new Random();

        private Point _position;
        private string _regionName;

        [JsonProperty(PropertyName = "Tile", NullValueHandling = NullValueHandling.Ignore)]
        private Tile _tile;

        [JsonProperty(PropertyName = "TileTemplate", NullValueHandling = NullValueHandling.Ignore)]
        private Tile _tileTemplate;

        /// <summary>
        /// This lock secures changes to the <see cref="Tile"/> property
        /// and its sub-properties.
        /// </summary>
        public Lock TileLock { get; private set; } = new Lock();

        /// <summary>
        /// This lock secures changes to the <see cref="TileTemplate"/>
        /// property.
        /// TODO: Do we need this? Where to implement?
        /// </summary>
        public Lock TileTemplateLock { get; private set; } = new Lock();

        /// <summary>
        /// This lock ensures that only one user can design the tile
        /// at the same time.
        /// </summary>
        public Lock DesignerLock { get; private set; } = new Lock();

        /// <summary>
        /// Refers to the <see cref="IMap"/> the <see cref="MapLocation"/>
        /// is associated with.
        /// </summary>
        public IMap Map { get; internal set; }

        /// <summary>
        /// Indicates whether the <see cref="MapLocation"/> is empty,
        /// i.e. whether both, <see cref="Tile"/> and <see cref="TileTemplate"/>
        /// are null.
        /// </summary>
        public bool IsEmpty => Tile == null && TileTemplate == null;

        /// <inheritdoc/>
        public Point Position
        {
            get { return _position; }
            internal set { Set(ref _position, value); }
        }

        /// <summary>
        /// Refers to a <see cref="Region"/> of the <see cref="IMap"/>.
        /// </summary>
        [JsonProperty]
        public string RegionName
        {
            get { return _regionName; }
            internal set { Set(ref _regionName, value); }
        }

        /// <inheritdoc/>
        public Tile Tile
        {
            get { return _tile; }
            set
            {
                if (Map != null)
                    throw new InvalidOperationException($"{nameof(Tile)} cannot be modified while attached to an {nameof(IMap)}");

                if (value == _tile)
                    return;

                if (_tile != null)
                    _tile.Location = null;

                if (value != null)
                    value.Location = this;

                Set(ref _tile, value);
            }
        }

        /// <summary>
        /// The <see cref="Tiles.Tile"/> that represents the gameplay state as
        /// experienced initially or after an area reset.
        /// This is the "designed" state.
        /// </summary>
        public Tile TileTemplate
        {
            get { return _tileTemplate; }
            internal set { Set(ref _tileTemplate, value); }
        }

        /// <summary>
        /// Restores the region that is made up of all connected
        /// <see cref="MapLocation"/>s with the same
        /// <see cref="RegionName"/>. This method can be called on
        /// any <see cref="MapLocation"/> within the same region.
        /// </summary>
        /// <returns></returns>
        public async Task RestoreRegionAsync()
        {
            if (Map == null)
                throw new InvalidOperationException();

            Region region;
            if (!Map.Metadata.Regions.TryGetValue(RegionName ?? "", out region))
            {
                Logger.LogWarning($"Cannot restore region: The region name '{RegionName}' does not correspond to a region entry in the map metadata");
                return;
            }

            var locations = await SelectRegionAsync((a, b) => a.RegionName == b.RegionName);
            var players = new List<PlayerEntity>();

            // In each location of the region:
            // 1) Detach the player
            // 2) Deactivate Tile and replace it with a copy of TileTemplate
            foreach (var location in locations)
            {
                if (location.Tile != null)
                {
                    var player = location.Tile.Entity as PlayerEntity;

                    if (player != null)
                    {
                        // Detach player from map (should always work thanks to 'forceDetach')
                        if (!await player.TryMoveAsync(null, new EntityMoveContext(Map, player), forceDetach: true))
                            throw new NotImplementedException("This should not have happened");

                        players.Add(player);
                    }

                    // Restore from template
                    await location.Tile.DeactivateAsync();
                }

                location.Tile = location.TileTemplate.DeepClone();
            }

            // Now activate all the shiny new Tiles
            foreach (var location in locations)
                await location.Tile?.ActivateAsync();

            // Re-attach players inside region's spawn area
            var spawnLocations = Map.Metadata.Regions[RegionName].PlayerSpawnArea.ToList();

            foreach (var player in players)
            {
                var spawnLocation = spawnLocations[_random.Next(spawnLocations.Count)];
                var context = new EntityMoveContext(Map, player);
                await player.TryMoveAsync(spawnLocation, context); // TODO: What if player respawn fails?
            }
        }

        /// <summary>
        /// Obtains <see cref="MapLocation"/>s, including this one,
        /// that form a cohesive region regarding a specific condition.
        /// </summary>
        /// <param name="condition">Locations are selected based on this condition</param>
        /// <param name="maxRadius">The maximum radius in which locations are selected</param>
        /// <returns></returns>
        internal async Task<MapLocation[]> SelectRegionAsync(SelectRegionCondition condition, int maxRadius = 20)
        {
            var directions = new[] { Point.West, Point.East, Point.North, Point.South };
            var visited = new HashSet<MapLocation>();
            var queue = new Queue<MapLocation>();
            queue.Enqueue(this);

            while (queue.Count != 0)
            {
                var location = queue.Dequeue();

                foreach (var dir in directions)
                {
                    var distance = Point.Distance(location.Position + dir, Position);

                    if (distance.X <= maxRadius && distance.Y <= maxRadius)
                    {
                        var neighbor = await Map.TryGetAsync(location.Position + dir);

                        if (!visited.Contains(neighbor) && condition(this, neighbor))
                        {
                            queue.Enqueue(neighbor);
                            visited.Add(neighbor);
                        }
                    }
                }
            }

            return visited.ToArray();
        }

        /// <summary>
        /// A condition that decides whether a <see cref="MapLocation"/>
        /// is selected during <see cref="SelectRegionAsync(SelectRegionCondition, int)"/>.
        /// </summary>
        /// <param name="start">The origin of the region selection</param>
        /// <param name="neighbor">The <see cref="MapLocation"/> to be checked</param>
        /// <returns></returns>
        public delegate bool SelectRegionCondition(MapLocation start, MapLocation neighbor);

        internal MapLocation DeepClone()
        {
            var clone = CloneWithoutHandlers(this);
            clone.Tile = Tile?.DeepClone();
            clone.TileTemplate = TileTemplate?.DeepClone();
            clone.RegionName = RegionName;
            TileLock = new Lock();
            TileTemplateLock = new Lock();
            DesignerLock = new Lock();
            return clone;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="MapLocation"/>.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString() => $"{Position} ({Tile?.ToString() ?? "no tile"}, {TileTemplate?.ToString() ?? "no template"})";
    }
}
