namespace UhlnocsServer.Models.Properties.Parameters.Infos
{
    public sealed class DoubleParameterInfo : ParameterInfo
    {
        public double DefaultValue { get; set; }

        public double MinValue { get; set; }

        public double MaxValue { get; set; }

        public DoubleParameterInfo(string id, string name, string description, double defaultValue,
                                   double minValue, double maxValue) : base(id, name, PropertyValueType.Double, description)
        {
            DefaultValue = defaultValue;
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
