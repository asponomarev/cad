using System.Text.Json;
using System.Text.Json.Nodes;

namespace UhlnocsServer.Models.Properties.Characteristics.Infos
{
    public abstract class ModelCharacteristicInfo : ModelPropertyInfo
    {
        public ModelCharacteristicInfo(string id, string name, ModelPropertyValueType valueType,
                                       string description) : base(id, name, valueType, description)
        {

        }

        public static ModelCharacteristicInfo FromJsonElement(JsonElement characteristicInfoElement)
        {
            ModelPropertyValueType valueType = characteristicInfoElement.GetProperty(nameof(ValueType)).Deserialize<ModelPropertyValueType>(PropertySerializerOptions);
            if (valueType == ModelPropertyValueType.Int)
            {
                return characteristicInfoElement.Deserialize<IntModelCharacteristicInfo>(PropertySerializerOptions);
            }
            return characteristicInfoElement.Deserialize<DoubleModelCharacteristicInfo>(PropertySerializerOptions);
        }

        public static JsonNode ToJsonNode(ModelCharacteristicInfo characteristicInfo)
        {
            string characteristicInfoJsonString = JsonSerializer.Serialize(characteristicInfo, characteristicInfo.GetType(), PropertySerializerOptions);
            return JsonNode.Parse(characteristicInfoJsonString);
        }
    }
}
