using System.Collections.Generic;
using System.Linq;

namespace OrangeBugReloaded.Core
{
    public interface IMapMetadata
    {
        IRegionTree Regions { get; }
        IPlayerCollection Players { get; }
        int TileVersion { get; }
        int TileMetadataVersion { get; }

        /// <summary>
        /// Increases the tile version by one and returns
        /// the new version.
        /// </summary>
        /// <returns>The new version</returns>
        int NextTileVersion();

        /// <summary>
        /// Increses the tile metadata version by one and
        /// returns the new version.
        /// </summary>
        /// <returns>The new version</returns>
        int NextTileMetadataVersion();
    }

    public interface IRegionTree
    {
        RegionInfo DefaultRegion { get; }
        RegionInfo GetBaseRegion(int id);
        IReadOnlyCollection<RegionInfo> GetDerivedRegions(int id);
        RegionInfo AddRegion(string name, int parentRegionId, Point spawnPosition);
        RegionInfo this[int id] { get; }
    }

    public interface IPlayerCollection
    {
        /// <summary>
        /// Checks whether the player with the specified
        /// ID is known in the context of the map.
        /// </summary>
        /// <param name="id">Player ID</param>
        /// <returns>True iff player is known</returns>
        bool IsKnown(string id);

        /// <summary>
        /// Adds a player to the list of known players.
        /// </summary>
        /// <param name="playerInfo">Player information</param>
        /// <exception cref="System.ArgumentException">
        /// A player with the specified ID is already known.
        /// </exception>
        void Add(PlayerInfo playerInfo);

        PlayerInfo this[string id] { get; set; }
    }

    public struct RegionInfo
    {
        public static RegionInfo Empty { get; } = new RegionInfo(null, int.MinValue, int.MinValue, Point.Zero);

        public static RegionInfo Default { get; } = new RegionInfo("Default", 0, int.MinValue, Point.Zero);

        public string Name { get; }

        public int Id { get; }

        public int Parent { get; }

        /// <summary>
        /// Specifies the area where a player respawns when resetting this region.
        /// The property value refers to a tile position on the map.
        /// The spawn area is formed by the tile at that position and all direct
        /// and indirect neighbors of the same region.
        /// </summary>
        public Point SpawnPosition { get; }

        public RegionInfo(string name, int id, int parent, Point spawnPosition)
        {
            Id = id;
            Name = name;
            Parent = parent;
            SpawnPosition = spawnPosition;
        }
    }

    public struct PlayerInfo
    {
        public static PlayerInfo Empty { get; } = new PlayerInfo();

        /// <summary>
        /// The player name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The ID of the player.
        /// </summary>
        /// <remarks>
        /// QUESTION: It is not yet clear what this actually is.
        /// An email address might work as it is unique.
        /// </remarks>
        public string Id { get; }

        /// <summary>
        /// The current position of the player on the map.
        /// If the player is not connected, this is the position
        /// where the player disconnected the last time.
        /// </summary>
        public Point Position { get; }

        public PlayerInfo(string id, string displayName, Point position)
        {
            Id = id;
            DisplayName = displayName;
            Position = position;
        }

        public PlayerInfo WithPosition(Point newPosition)
            => new PlayerInfo(Id, DisplayName, newPosition);
    }
}
