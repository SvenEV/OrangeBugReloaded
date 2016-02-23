using Newtonsoft.Json;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Provides information about a tile on the map in its
    /// designed state. Additionally provides metadata about
    /// the tile.
    /// </summary>
    public struct TileMetadata
    {
        public static TileMetadata Empty => new TileMetadata(Tile.Empty, 0);

        public Tile TileTemplate { get; }
        public int RegionId { get; }

        [JsonConstructor]
        public TileMetadata(Tile tileTemplate, int regionId) : this()
        {
            TileTemplate = tileTemplate.EnsureNotNull();
            RegionId = regionId;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(TileMetadata))
                return false;

            var o = (TileMetadata)obj;

            return Equals(TileTemplate, o.TileTemplate) && RegionId == o.RegionId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(TileMetadata a, TileMetadata b) => Equals(a, b);
        public static bool operator !=(TileMetadata a, TileMetadata b) => !Equals(a, b);
    }
}
