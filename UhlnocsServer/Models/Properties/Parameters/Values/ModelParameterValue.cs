namespace UhlnocsServer.Models.Properties.Parameters.Values
{
    public abstract class ModelParameterValue : ModelPropertyValue
    {
        public ModelParameterValue(string id, string name, ModelPropertyValueType valueType) : base(id, name, valueType)
        {

        }
    }
}
