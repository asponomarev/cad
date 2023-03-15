using System.Text.Json;
using System.Text.Json.Serialization;

namespace UhlnocsServer.Models.Properties
{
    public abstract class ModelProperty
    {
        public static readonly JsonSerializerOptions PropertySerializerOptions = new()
        {
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        public string Id { get; set; }

        public string Name { get; set; }

        public ModelPropertyValueType ValueType { get; set; }

        public ModelProperty(string id, string name, ModelPropertyValueType valueType)
        {
            Id = id;
            Name = name;
            ValueType = valueType;
        }
    }
}
