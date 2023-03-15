namespace UhlnocsServer.Models.Properties.Parameters.Values
{
    public sealed class BoolModelParameterValue : ModelParameterValue
    {
        public bool Value { get; set; }

        public BoolModelParameterValue(string id, string name,
                                       bool value) : base(id, name, ModelPropertyValueType.Bool)
        {
            Value = value;
        }
    }
}
