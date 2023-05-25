namespace UhlnocsServer.Models.Properties.Characteristics
{
    public sealed class CharacteristicWithModel : PropertyBase
    {
        public string? Model { get; set; }

        public bool? ModelSetByUser { get; set; }

        public CharacteristicWithModel(string id, string? model, bool? modelSetByUser = false) : base(id)
        {
            Model = model;
            ModelSetByUser = modelSetByUser;
        }
    }
}
