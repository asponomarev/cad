namespace UhlnocsServer.Models.Properties.Parameters.Infos
{
    public sealed class BoolModelParameterInfo : ModelParameterInfo
    {
        public bool DefaultValue { get; set; }

        public BoolModelParameterInfo(string id, string name, string description,
                                      bool defaultValue) : base(id, name, ModelPropertyValueType.Bool, description)
        {
            DefaultValue = defaultValue;
        }
    }
}
