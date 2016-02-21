using System.Collections.Generic;

namespace OrangeBugReloaded.Core.Events
{
    public interface ILocatedGameEvent : IGameEvent
    {
        IEnumerable<Point> GetPositions();
    }
}
