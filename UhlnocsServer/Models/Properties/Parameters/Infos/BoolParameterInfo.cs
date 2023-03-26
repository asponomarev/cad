namespace UhlnocsServer.Models.Properties.Parameters.Infos
{
    public sealed class BoolParameterInfo : ParameterInfo
    {
        public bool DefaultValue { get; set; }

        public BoolParameterInfo(string id, string name, string description,
                                 bool defaultValue) : base(id, name, PropertyValueType.Bool, description)
        {
            DefaultValue = defaultValue;
        }
    }
}
