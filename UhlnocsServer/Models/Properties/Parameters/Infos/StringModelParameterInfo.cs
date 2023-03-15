namespace UhlnocsServer.Models.Properties.Parameters.Infos
{
    public sealed class StringModelParameterInfo : ModelParameterInfo
    {
        public string DefaultValue { get; set; }

        public List<string> PossibleValues { get; set; }

        public StringModelParameterInfo(string id, string name, string description, string defaultValue, 
                                        List<string> possibleValues) : base(id, name, ModelPropertyValueType.String, description)
        {
            DefaultValue = defaultValue;
            PossibleValues = possibleValues;
        }
    }
}
