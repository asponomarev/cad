namespace UhlnocsServer.Models.Properties
{
    public abstract class PropertyInfo : Property
    {
        public string Description { get; set; }

        public PropertyInfo(string id, string name, PropertyValueType valueType, 
                            string description) : base(id, name, valueType)
        {
            Description = description;
        }
    }
}
