using DSS.Extensions;
using DSS.Models;
using DSS.Wrappers;
using UhlnocsServer.Calculations;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties.Parameters.Infos;
using UhlnocsServer.Models.Properties.Parameters.Values;

namespace DSS
{
    public class ParameterFinder : IParameterFinder
    {
        private readonly IServerWrapper _serverWrapper;
        private readonly INearestNeighborsFinder _nearestNeighborsFinder;

        public ParameterFinder(IServerWrapper serverWrapper, INearestNeighborsFinder nearestNeighborsFinder) 
        {
            _serverWrapper = serverWrapper;
            _nearestNeighborsFinder = nearestNeighborsFinder;
        }

        public IList<ParameterValue> GetMatching(IList<ParameterValue> knownParameters, string modelId)
        {
            var resultParameters = new List<ParameterValue>();

            var suitableСonfigurations = GetSuitableСonfigurations(knownParameters, modelId);

            var matchingConfiguration = suitableСonfigurations.FirstOrDefault(config
                => knownParameters
                .Join(
                    config.Parameters,
                    known => new { known.Id, Value = known.GetValue() },
                    configParam => new { configParam.Id, Value = configParam.GetValue() },
                    (known, configParam) => known)
                .Count() == knownParameters.Count);

            if (matchingConfiguration != null)
            {
                resultParameters.AddRange(knownParameters);
                foreach (var parameter in matchingConfiguration.Parameters)
                {
                    if (!resultParameters.Any(p => p.Id.Equals(parameter.Id)))
                    {
                        if (parameter is IntParameterValue intParameter)
                        {
                            resultParameters.Add(new IntParameterValue(intParameter.Id, intParameter.Value));
                        }
                        else if (parameter is DoubleParameterValue doubleParameter)
                        {
                            resultParameters.Add(new DoubleParameterValue(doubleParameter.Id, doubleParameter.Value));
                        }
                        else if (parameter is BoolParameterValue boolParameter)
                        {
                            resultParameters.Add(new BoolParameterValue(boolParameter.Id, boolParameter.Value));
                        }
                        else if (parameter is StringParameterValue stringParameter)
                        {
                            resultParameters.Add(new StringParameterValue(stringParameter.Id, stringParameter.Value));
                        }
                    }
                }
            }
            return resultParameters;
        }

        public IList<ParameterValue> GetNearest(IList<ParameterValue> knownParameters, string modelId, double searchAccuracy)
        {
            var resultParameters = new List<ParameterValue>();

            var suitableСonfigurations = GetSuitableСonfigurations(knownParameters, modelId);

            var nearestConfigurations = suitableСonfigurations.Where(config
                => knownParameters
                .Join(
                    config.Parameters,
                    known => known.Id,
                    configParam => configParam.Id,
                    (known, configParam) => new JoinedParameters(known, configParam))
                .Where(pair => pair.CompareParameters(searchAccuracy))
                .Count() == knownParameters.Count).ToList();

            if (nearestConfigurations.Any())
            {
                resultParameters.AddRange(knownParameters);
                var nearestParameters = nearestConfigurations.Select(c => c.Parameters).ToList();
                var nearestNeighbors = _nearestNeighborsFinder.Find(nearestParameters, new List<ParameterValue>(knownParameters), 5);
                
                var groupingNearestNeighbors = nearestNeighbors.SelectMany(config => config).GroupBy(p => p.Id);
                foreach (var param in groupingNearestNeighbors)
                {
                    if (!resultParameters.Any(p => p.Id.Equals(param.Key)))
                    {
                        var firstParameterInGroup = param.FirstOrDefault();
                        if (firstParameterInGroup is IntParameterValue)
                        {
                            resultParameters.Add(new IntParameterValue(firstParameterInGroup.Id, param.Sum(p => (int)p.GetValue()) / param.Count()));
                        }
                        else if (firstParameterInGroup is DoubleParameterValue)
                        {
                            resultParameters.Add(new DoubleParameterValue(firstParameterInGroup.Id, param.Sum(p => (double)p.GetValue()) / param.Count()));
                        }
                        else if (firstParameterInGroup is BoolParameterValue)
                        {
                            var value = param.Select(p => (bool)p.GetValue())
                                .GroupBy(x => x)
                                .OrderByDescending(g => g.Count())
                                .Select(g => g.Key)
                                .First();
                            resultParameters.Add(new BoolParameterValue(firstParameterInGroup.Id, value));
                        }
                        else if (firstParameterInGroup is StringParameterValue)
                        {
                            var value = param.Select(p => (string)p.GetValue())
                                .GroupBy(x => x)
                                .OrderByDescending(g => g.Count())
                                .Select(g => g.Key)
                                .First();
                            resultParameters.Add(new StringParameterValue(firstParameterInGroup.Id, value));
                        }
                    }
                }
            }
            return resultParameters;
        }

        public IList<ParameterValue> GetDefault(IList<ParameterValue> knownParameters, IList<ParameterInfo> parameterInfos)
        {
            var resultParameters = new List<ParameterValue>();
            resultParameters.AddRange(knownParameters);
            foreach (var parameter in parameterInfos)
            {
                if (!resultParameters.Any(p => p.Id.Equals(parameter.Id)))
                {
                    if (parameter is IntParameterInfo intParameterInfo)
                    {
                        resultParameters.Add(new IntParameterValue(intParameterInfo.Id, intParameterInfo.DefaultValue));
                    }
                    else if (parameter is DoubleParameterInfo doubleParameterInfo)
                    {
                        resultParameters.Add(new DoubleParameterValue(doubleParameterInfo.Id, doubleParameterInfo.DefaultValue));
                    }
                    else if (parameter is BoolParameterInfo boolParameterInfo)
                    {
                        resultParameters.Add(new BoolParameterValue(boolParameterInfo.Id, boolParameterInfo.DefaultValue));
                    }
                    else if (parameter is StringParameterInfo stringParameterInfo)
                    {
                        resultParameters.Add(new StringParameterValue(stringParameterInfo.Id, stringParameterInfo.DefaultValue));
                    }
                }
            }
            return resultParameters;
        }

        private IEnumerable<LaunchConfiguration> GetSuitableСonfigurations(IList<ParameterValue> knownParameters, string modelId)
        {
            return _serverWrapper.GetLaunchConfigurations()
                        .Where(l => l.Characteristics.Select(c => c.Model).Contains(modelId))
                        .Where(l => l.Parameters.Select(p => p.Id)
                            .Intersect(knownParameters.Select(p => p.Id))
                            .Count() == knownParameters.Count);
        }
    }
}
