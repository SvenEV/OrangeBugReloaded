using Newtonsoft.Json;
using OrangeBugReloaded.Core.Foundation;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Base class for Orange Bug Game Objects.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [JsonConverter(typeof(OBGOConverter))]
    public abstract class OrangeBugGameObject : BindableBase
    {
        [JsonProperty(PropertyName = "$type", Order = -2)]
        private string Type => GetType().Name;

        /// <summary>
        /// A completed task.
        /// </summary>
        public static Task Done { get; } = TaskEx.FromResult(0);

        /// <summary>
        /// A completed task with result 'true'.
        /// </summary>
        public static Task<bool> True { get; } = TaskEx.FromResult(true);

        /// <summary>
        /// A completed task with result 'false'.
        /// </summary>
        public static Task<bool> False { get; } = TaskEx.FromResult(false);
    }
}
