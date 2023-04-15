using UhlnocsServer.Calculations;
using UhlnocsServer.Models.Properties.Parameters;

namespace DSS
{
    public interface IParameterFinder
    {
        public IList<ParameterValue> GetMatching(IList<ParameterValue> knownParameters, string modelId);

        public IList<ParameterValue> GetNearest(IList<ParameterValue> knownParameters, string modelId, double searchAccuracy);

        public IList<ParameterValue> GetDefault(IList<ParameterValue> knownParameters, IList<ParameterInfo> parameterInfos);
    }
}
