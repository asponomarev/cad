namespace UhlnocsServer.Models.Properties.Parameters.Values
{
    public sealed class BoolParameterValue : ParameterValue
    {
        public bool Value { get; set; }

        public BoolParameterValue(string id, bool value) : base(id)
        {
            Value = value;
        }
    }
}
