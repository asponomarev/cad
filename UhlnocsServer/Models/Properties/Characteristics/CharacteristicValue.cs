using UhlnocsServer.Models.Properties.Characteristics.Values;

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
                    return (characteristic as DoubleCharacteristicValue).Value;
                }
            }
            throw new Exception($"Characteristic with id {throughputCharacteristicId} not found in list {characteristics}");
        }
    }
}
