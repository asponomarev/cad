namespace UhlnocsServer.Models.Properties
{
    public abstract class ModelPropertyInfo : ModelProperty
    {
        public string Description { get; set; }

        public ModelPropertyInfo(string id, string name, ModelPropertyValueType valueType,
                                 string description) : base(id, name, valueType)
        {
            Description = description;
        }
    }
}
