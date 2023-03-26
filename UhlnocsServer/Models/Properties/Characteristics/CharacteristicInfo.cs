using System.Text.Json;
using System.Text.Json.Nodes;
using UhlnocsServer.Models.Properties.Characteristics.Infos;

namespace UhlnocsServer.Models.Properties.Characteristics
{
    public abstract class CharacteristicInfo : PropertyInfo
    {
        public CharacteristicInfo(string id, string name, PropertyValueType valueType,
                                  string description) : base(id, name, valueType, description)
        {

        }

        public static CharacteristicInfo FromJsonElement(JsonElement characteristicInfoElement)
        {
            PropertyValueType valueType = characteristicInfoElement.GetProperty(nameof(ValueType)).Deserialize<PropertyValueType>(PropertySerializerOptions);
            if (valueType == PropertyValueType.Int)
            {
                return characteristicInfoElement.Deserialize<IntCharacteristicInfo>(PropertySerializerOptions);
            }
            return characteristicInfoElement.Deserialize<DoubleCharacteristicInfo>(PropertySerializerOptions);
        }

        public static JsonNode ToJsonNode(CharacteristicInfo characteristicInfo)
        {
            string characteristicInfoJsonString = JsonSerializer.Serialize(characteristicInfo, characteristicInfo.GetType(), PropertySerializerOptions);
            return JsonNode.Parse(characteristicInfoJsonString);
        }
    }
}
