using System;
using System.Collections.Generic;

namespace OrangeBugReloaded.Core
{
    public class MapMetadata : IMapMetadata
    {
        private object _lock = new object();

        public string Name { get; }
        public RegionInfo RootRegion { get; } = new RegionInfo(0, "Root region", new Rectangle(-8, 0, 2 * Chunk.Size - 1, Chunk.Size - 1));
        public IPlayerCollection Players { get; } = new PlayerCollection();
        public int Version { get; private set; }

        public int NextVersion()
        {
            lock (_lock)
                return ++Version;
        }
    }

    public class PlayerCollection : IPlayerCollection
    {
        // Maps player IDs to PlayerInfos
        private Dictionary<string, PlayerInfo> _players = new Dictionary<string, PlayerInfo>();

        public bool IsKnown(string playerId) => _players.ContainsKey(playerId);

        public void Add(PlayerInfo player)
        {
            if (_players.ContainsKey(player.Id))
                throw new ArgumentException("A player with the specified ID is already known");

            _players.Add(player.Id, player);
        }

        public void UpdatePosition(string playerId, Point position)
        {
            PlayerInfo player;

            if (_players.TryGetValue(playerId, out player))
            {
                _players[playerId] = new PlayerInfo(player.Id, player.DisplayName, position);
            }
            else
            {
                throw new ArgumentException($"The player with ID '{playerId}' is not known");
            }
        }

        public PlayerInfo this[string id]
        {
            get { return _players[id]; }
            set { _players[id] = value; }
        }
    }
}
