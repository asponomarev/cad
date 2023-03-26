namespace UhlnocsServer.Models.Properties.Characteristics.Infos
{
    public sealed class IntCharacteristicInfo : CharacteristicInfo
    {
        public IntCharacteristicInfo(string id, string name,
                                     string description) : base(id, name, PropertyValueType.Int, description)
        {

        }
    }
}
