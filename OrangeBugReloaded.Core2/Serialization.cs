using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;

namespace OrangeBugReloaded.Core
{
    public class Serialization
    {
        public static JsonSerializerSettings JsonSerializationSettings { get; } = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };
    }
}
