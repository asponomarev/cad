namespace UhlnocsServer.Models.Properties.Parameters.Values
{
    public sealed class IntParameterValue : ParameterValue
    {
        public int Value { get; set; }

        public IntParameterValue(string id, int value) : base(id)
        {
            Value = value;
        }
    }
}
