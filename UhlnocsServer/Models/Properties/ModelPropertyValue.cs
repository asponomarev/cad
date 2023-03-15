namespace UhlnocsServer.Models.Properties
{
    public abstract class ModelPropertyValue : ModelProperty
    {
        public ModelPropertyValue(string id, string name, ModelPropertyValueType valueType) : base(id, name, valueType)
        {

        }
    }
}
