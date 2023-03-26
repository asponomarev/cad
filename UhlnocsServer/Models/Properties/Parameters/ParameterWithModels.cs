namespace UhlnocsServer.Models.Properties.Parameters
{
    public class ParameterWithModels : PropertyWithModels
    {
        public ParameterWithModels(string id, string name, PropertyValueType valueType,
                                   List<string> models) : base(id, name, valueType, models)
        {

        }
    }
}
