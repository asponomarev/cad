namespace UhlnocsServer.Models.Properties
{
    public abstract class Property : PropertyBase
    {
        public string Name { get; set; }

        public PropertyValueType ValueType { get; set; }

        public Property(string id, string name, PropertyValueType valueType) : base(id)
        {
            Name = name;
            ValueType = valueType;
        }
    }
}
