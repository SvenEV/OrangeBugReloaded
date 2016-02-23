using Newtonsoft.Json;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Provides information about a tile during gameplay.
    /// </summary>
    public struct TileInfo
    {
        public static TileInfo Empty { get; } = new TileInfo();

        public Tile Tile { get; }
        public int Version { get; }

        [JsonConstructor]
        public TileInfo(Tile tile, int version)
        {
            Tile = tile;
            Version = version;
        }

        public TileInfo WithTile(Tile newTile) => new TileInfo(newTile, Version);
        public TileInfo WithVersion(int newVersion) => new TileInfo(Tile, newVersion);

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(TileInfo))
                return false;

            var other = (TileInfo)obj;
            return Equals(Tile, other.Tile) && Version == other.Version;
        }

        public override int GetHashCode()
        {
            return new { Tile, Version }.GetHashCode();
        }

        public static bool operator ==(TileInfo a, TileInfo b) => Equals(a, b);
        public static bool operator !=(TileInfo a, TileInfo b) => !Equals(a, b);

        public override string ToString() => $"[{Version.ToString().PadLeft(4, '0')}] {Tile.ToString()}";
    }
}
