namespace UhlnocsServer.Models.Properties.Parameters.Values
{
    public sealed class StringModelParameterValue : ModelParameterValue
    {
        public string Value { get; set; }

        public StringModelParameterValue(string id, string name,
                                         string value) : base(id, name, ModelPropertyValueType.Int)
        {
            Value = value;
        }
    }
}
