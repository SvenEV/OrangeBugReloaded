using Newtonsoft.Json;
using OrangeBugReloaded.Core.Entities;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// A Region defines an arbitrarily shaped area on an <see cref="IMap"/>.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Region
    {
        /// <summary>
        /// Gets or sets the permissions for the <see cref="Region"/>.
        /// </summary>
        [JsonProperty]
        public RegionPermissions Permissions { get; set; }

        /// <summary>
        /// The area where <see cref="PlayerEntity"/> instances will be
        /// teleported to when the <see cref="Region"/> is restored.
        /// </summary>
        [JsonProperty]
        public Rectangle PlayerSpawnArea { get; set; }
    }
}
