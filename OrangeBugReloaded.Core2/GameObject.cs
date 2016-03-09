using Newtonsoft.Json;

namespace OrangeBugReloaded.Core
{
    public class GameObject
    {
        [JsonIgnore]
        public virtual string VisualKey => GetType().Name;

        [JsonIgnore]
        public virtual Point VisualOrientation => Point.Zero;
    }
}
