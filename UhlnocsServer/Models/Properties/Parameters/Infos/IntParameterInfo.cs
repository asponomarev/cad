namespace UhlnocsServer.Models.Properties.Parameters.Infos
{
    public sealed class IntParameterInfo : ParameterInfo
    {
        public int DefaultValue { get; set; }

        public int MinValue { get; set; }

        public int MaxValue { get; set; }

        public IntParameterInfo(string id, string name, string description, int defaultValue,
                                int minValue, int maxValue) : base(id, name, PropertyValueType.Int, description)
        {
            DefaultValue = defaultValue;
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
