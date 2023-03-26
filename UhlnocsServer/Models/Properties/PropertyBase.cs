using System.Text.Json;
using System.Text.Json.Serialization;

namespace UhlnocsServer.Models.Properties
{
    public abstract class PropertyBase
    {
        public static readonly JsonSerializerOptions PropertySerializerOptions = new()
        {
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        public string Id { get; set; }

        public PropertyBase(string id)
        {
            Id = id;
        }
    }
}
