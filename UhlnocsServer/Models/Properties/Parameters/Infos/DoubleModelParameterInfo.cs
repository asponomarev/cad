namespace UhlnocsServer.Models.Properties.Parameters.Infos
{
    public sealed class DoubleModelParameterInfo : ModelParameterInfo
    {
        public double DefaultValue { get; set; }

        public double MinValue { get; set; }

        public double MaxValue { get; set; }

        public DoubleModelParameterInfo(string id, string name, string description, double defaultValue,
                                        double minValue, double maxValue) : base(id, name, ModelPropertyValueType.Double, description)
        {
            DefaultValue = defaultValue;
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
