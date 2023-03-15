namespace UhlnocsServer.Models.Properties.Parameters.Values
{
    public sealed class DoubleModelParameterValue : ModelParameterValue
    {
        public double Value { get; set; }

        public DoubleModelParameterValue(string id, string name,
                                         double value) : base(id, name, ModelPropertyValueType.Double)
        {
            Value = value;
        }
    }
}
