namespace UhlnocsServer.Models.Properties.Parameters.Values
{
    public sealed class DoubleParameterValue : ParameterValue
    {
        public double Value { get; set; }

        public DoubleParameterValue(string id, double value) : base(id)
        {
            Value = value;
        }
    }
}
