namespace UhlnocsServer.Calculations.LaunchResult
{
    // list of this class objects is a part of LaunchResult and contains info about characteristic and its values
    public class CharacteristicResult
    {
        public string Id { get; set; }

        public string CalculatedByModel { get; set; }

        public List<object?> Values { get; set; }

        public CharacteristicResult(string id, string calculatedByModel, List<object?> values)
        {
            Id = id;
            CalculatedByModel = calculatedByModel;
            Values = values;
        }
    }
}
