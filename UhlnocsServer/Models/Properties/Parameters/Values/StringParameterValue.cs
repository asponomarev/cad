namespace UhlnocsServer.Models.Properties.Parameters.Values
{
    public sealed class StringParameterValue : ParameterValue
    {
        public string Value { get; set; }

        public StringParameterValue(string id, string value) : base(id)
        {
            Value = value;
        }
    }
}
