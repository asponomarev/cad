namespace UhlnocsServer.Models.Properties.Characteristics
{
    public sealed class CharacteristicWithModel : PropertyBase
    {
        public string Model { get; set; }

        public CharacteristicWithModel(string id, string model) : base(id)
        {
            Model = model;
        }
    }
}
