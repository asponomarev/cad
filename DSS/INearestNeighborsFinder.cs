using UhlnocsServer.Models.Properties.Parameters;

namespace DSS
{
    public interface INearestNeighborsFinder
    {
        public List<List<ParameterValue>> Find(List<List<ParameterValue>> parametersList, List<ParameterValue> knownParameters, int k);
    }
}
