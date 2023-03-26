namespace UhlnocsServer.Models.Properties.Characteristics.Values
{
    public sealed class IntCharacteristicValue : CharacteristicValue
    {
        public int Value { get; set; }

        public IntCharacteristicValue(string id, int value) : base(id)
        {
            Value = value;
        }
    }
}
