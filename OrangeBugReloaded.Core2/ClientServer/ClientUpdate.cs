using OrangeBugReloaded.Core.Events;
using System.Collections.Generic;

namespace OrangeBugReloaded.Core.ClientServer
{
    public class ClientUpdate
    {
        public IReadOnlyCollection<TileUpdate> TileUpdates { get; }

        public IReadOnlyCollection<IGameEvent> Events { get; }

        public ClientUpdate(IReadOnlyCollection<TileUpdate> tileUpdates, IReadOnlyCollection<IGameEvent> events)
        {
            TileUpdates = tileUpdates;
            Events = events;
        }
    }
}
