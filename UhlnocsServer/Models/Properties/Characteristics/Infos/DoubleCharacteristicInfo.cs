namespace UhlnocsServer.Models.Properties.Characteristics.Infos
{
    public sealed class DoubleCharacteristicInfo : CharacteristicInfo
    {
        public DoubleCharacteristicInfo(string id, string name,
                                        string description) : base(id, name, PropertyValueType.Double, description)
        {

        }
    }
}
