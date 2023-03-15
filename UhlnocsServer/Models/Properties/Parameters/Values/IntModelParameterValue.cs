namespace UhlnocsServer.Models.Properties.Parameters.Values
{
    public sealed class IntModelParameterValue : ModelParameterValue
    {
        public int Value { get; set; }

        public IntModelParameterValue(string id, string name,
                                      int value) : base(id, name, ModelPropertyValueType.Int)
        {
            Value = value;
        }
    }
}
