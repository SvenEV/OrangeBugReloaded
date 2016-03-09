using Newtonsoft.Json;
using OrangeBugReloaded.Core.Presentation;

namespace OrangeBugReloaded.Core
{
    public class GameObject : IVisualHint
    {
        [JsonIgnore]
        public virtual string VisualKey => GetType().Name;

        [JsonIgnore]
        public virtual Point VisualOrientation => Point.Zero;
    }
}
