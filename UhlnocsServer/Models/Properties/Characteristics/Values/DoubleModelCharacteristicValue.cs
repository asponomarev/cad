namespace UhlnocsServer.Models.Properties.Characteristics.Values
{
    public sealed class DoubleModelCharacteristicValue : ModelCharacteristicValue
    {
        public double Value { get; set; }

        public DoubleModelCharacteristicValue(string id, string name,
                                              double value) : base(id, name, ModelPropertyValueType.Double)
        {
            Value = value;
        }
    }
}
