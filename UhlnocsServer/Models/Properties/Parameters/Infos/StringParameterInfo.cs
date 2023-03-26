namespace UhlnocsServer.Models.Properties.Parameters.Infos
{
    public sealed class StringParameterInfo : ParameterInfo
    {
        public string DefaultValue { get; set; }

        public List<string> PossibleValues { get; set; }

        public StringParameterInfo(string id, string name, string description, string defaultValue, 
                                   List<string> possibleValues) : base(id, name, PropertyValueType.String, description)
        {
            DefaultValue = defaultValue;
            PossibleValues = possibleValues;
        }
    }
}
