using System.Collections.Generic;

namespace OrangeBugReloaded.Core
{
    public interface IMapMetadata
    {
        RegionInfo RootRegion { get; }
        IPlayerCollection Players { get; }
        int Version { get; }

        /// <summary>
        /// Increases the version by one and returns
        /// the new version.
        /// </summary>
        /// <returns>The new version</returns>
        int NextVersion();
    }

    public interface IRegionCollection
    {
        RegionInfo this[int id] { get; set; }
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

        PlayerInfo this[string id] { get; }
    }

    public class RegionInfo
    {
        public string Name { get; }

        public int Id { get; }

        public IReadOnlyCollection<RegionInfo> Children { get; }

        public Rectangle SpawnArea { get; }

        public RegionInfo(int id, string name, Rectangle spawnArea)
        {
            Id = id;
            Name = name;
            SpawnArea = spawnArea;
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
    }
}
