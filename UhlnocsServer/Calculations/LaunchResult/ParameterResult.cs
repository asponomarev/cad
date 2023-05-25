namespace UhlnocsServer.Calculations.LaunchResult
{
    // list of this class objects is a part of LaunchResult and contains info about parameter and its values
    public class ParameterResult
    {
        public string Id { get; set; }

        public List<string> UsedByModels { get; set; }

        public List<object?> Values { get; set; }

        public ParameterResult(string id, List<string> usedByModels, List<object?> values)
        {
            Id = id;
            UsedByModels = usedByModels;
            Values = values;
        }
    }
}
