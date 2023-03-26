namespace UhlnocsServer.Models.Properties.Characteristics.Values
{
    public sealed class DoubleCharacteristicValue : CharacteristicValue
    {
        public double Value { get; set; }

        public DoubleCharacteristicValue(string id, double value) : base(id)
        {
            Value = value;
        }
    }
}
