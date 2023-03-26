namespace UhlnocsServer.Models.Properties.Characteristics
{
    public class CharacteristicWithModels : PropertyWithModels
    {
        public CharacteristicWithModels(string id, string name, PropertyValueType valueType,
                                        List<string> models) : base(id, name, valueType, models)
        {

        }
    }
}
