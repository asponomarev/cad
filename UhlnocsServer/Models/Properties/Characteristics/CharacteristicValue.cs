using System.Text.Json;
using UhlnocsServer.Models.Properties.Characteristics.Values;
using UhlnocsServer.Services;

namespace UhlnocsServer.Models.Properties.Characteristics
{
    public abstract class CharacteristicValue : PropertyValue
    {
        public CharacteristicValue(string id) : base(id)
        {

        }

        public static double GetThroughputValue(List<CharacteristicValue> characteristics, string throughputCharacteristicId)
        {
            foreach (CharacteristicValue characteristic in characteristics)
            {
                if (characteristic.Id == throughputCharacteristicId)
                {
                    return ((DoubleCharacteristicValue)characteristic).Value;
                }
            }
            throw new Exception($"Characteristic with id {throughputCharacteristicId} not found in list {characteristics}");
        }

        public static object? GetValue(CharacteristicValue characteristic)
        {
            if (characteristic is IntCharacteristicValue intCharacteristic)
            {
                return intCharacteristic.Value;
            }
            if (characteristic is DoubleCharacteristicValue doubleCharacteristic) 
            { 
                return doubleCharacteristic.Value;
            }
            return null;
        }

        public static List<CharacteristicValue> ListFromJsonElement(JsonElement characteristicsElement)
        {
            List<CharacteristicValue> characteristics = new();
            foreach (JsonElement characteristicElement in characteristicsElement.EnumerateArray())
            {
                characteristics.Add(FromJsonElement(characteristicElement));
            }
            return characteristics;
        }

        public static CharacteristicValue FromJsonElement(JsonElement characteristicElement)
        {
            string characteristicId = characteristicElement.GetProperty(nameof(Id)).GetString();
            PropertyValueType characteristicValueType = ModelService.CharacteristicsWithModels[characteristicId].ValueType;
            if (characteristicValueType == PropertyValueType.Int)
            {
                return characteristicElement.Deserialize<IntCharacteristicValue>(PropertySerializerOptions);
            }
            return characteristicElement.Deserialize<DoubleCharacteristicValue>(PropertySerializerOptions);
        }
    }
}
