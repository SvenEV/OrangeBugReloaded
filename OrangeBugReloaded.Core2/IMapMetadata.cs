namespace OrangeBugReloaded.Core
{
    public interface IMapMetadata
    {
        IRegionCollection Regions { get; }
        IPlayerCollection Players { get; }
    }

    public interface IRegionCollection
    {
        RegionInfo this[int id] { get; set; }
    }

    public interface IPlayerCollection
    {
        PlayerInfo this[string id] { get; set; }
    }

    public struct RegionInfo
    {
        public string Name { get; }

        public int Id { get; }

        public RegionInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public struct PlayerInfo
    {
        /// <summary>
        /// The player name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The ID of the player.
        /// </summary>
        /// <remarks>
        /// TODO: It is not yet clear what this actually is.
        /// An email address might work as it is unique.
        /// </remarks>
        public string Id { get; }

        public PlayerInfo(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
