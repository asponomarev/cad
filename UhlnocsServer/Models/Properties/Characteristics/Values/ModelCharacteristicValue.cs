namespace UhlnocsServer.Models.Properties.Characteristics.Values
{
    public abstract class ModelCharacteristicValue : ModelPropertyValue
    {
        public ModelCharacteristicValue(string id, string name, ModelPropertyValueType valueType) : base(id, name, valueType)
        {

        }
    }
}
