using UhlnocsServer.Models.Properties.Parameters;

namespace DSS
{
    public interface IParameterFinder
    {
        public Task<IList<ParameterValue>> GetMatching(IList<ParameterValue> knownParameters, string modelId);

        public Task<IList<ParameterValue>> GetNearest(IList<ParameterValue> knownParameters, string modelId, double searchAccuracy);

        public IList<ParameterValue> GetDefault(IList<ParameterValue> knownParameters, IList<ParameterInfo> parameterInfos);
    }
}
