using Newtonsoft.Json;

namespace OrangeBugReloaded.Core.ClientServer
{
    public struct TileUpdate
    {
        public Point Position { get; }

        public TileInfo TileInfo { get; }

        [JsonConstructor]
        public TileUpdate(Point position, TileInfo tileInfo)
        {
            Position = position;
            TileInfo = tileInfo;
        }
    }
}
