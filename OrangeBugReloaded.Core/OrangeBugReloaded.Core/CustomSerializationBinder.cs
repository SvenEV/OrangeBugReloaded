using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using OrangeBugReloaded.Core.Foundation;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// OrangeBugGameObject converter that handles the short '$type' property
    /// written by <see cref="OrangeBugGameObject.Type"/> in order to
    /// instantiate the appropiate Tile or Entity.
    /// </summary>
    class OBGOConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(OrangeBugGameObject).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                // Read the '$type' property
                var jo = JObject.Load(reader);
                var typeName = jo["$type"].Value<string>();

                try
                {
                    // Create instance of the type and populate with the remaining JSON properties
                    var type = Reflector.Default.GetType(typeName);
                    var result = Activator.CreateInstance(type);
                    serializer.Populate(jo.CreateReader(), result);
                    return result;
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    return null;
                }
            }
            else return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
