namespace UhlnocsServer.Models.Properties.Characteristics.Values
{
    public sealed class IntModelCharacteristicValue : ModelCharacteristicValue
    {
        public int Value { get; set; }

        public IntModelCharacteristicValue(string id, string name,
                                           int value) : base(id, name, ModelPropertyValueType.Int)
        {
            Value = value;
        }
    }
}
