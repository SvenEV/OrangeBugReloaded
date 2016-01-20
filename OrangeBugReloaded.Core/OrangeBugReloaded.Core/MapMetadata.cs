using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Contains metadata for an <see cref="IMap"/>.
    /// TODO :)
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MapMetadata
    {
        /// <summary>
        /// The username of the map creator.
        /// </summary>
        [JsonProperty]
        public string Creator { get; set; }

        /// <summary>
        /// The creation date.
        /// </summary>
        [JsonProperty]
        public DateTimeOffset CreationDate { get; set; }

        /// <summary>
        /// Contains the region-declarations.
        /// </summary>
        [JsonProperty]
        public Dictionary<string, Region> Regions { get; set; }
    }
}
