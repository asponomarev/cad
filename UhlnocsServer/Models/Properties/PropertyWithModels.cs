namespace UhlnocsServer.Models.Properties
{
    public class PropertyWithModels : Property
    {
        public List<string> Models { get; set; }

        public PropertyWithModels(string id, string name, PropertyValueType valueType,
                                  List<string> models) : base(id, name, valueType)
        {
            Models = models;
        }
    }
}
