using DSS.Wrappers;
using UhlnocsServer.Calculations;
using UhlnocsServer.Models;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Optimizations;
using UhlnocsServer.Services;

namespace DSS
{
    public class ConfigurationEnhancer : IConfigurationEnhancer
    {
        private readonly IModelSelector _modelSelector;
        private readonly IParameterFinder _parameterFinder;
        private readonly IServerWrapper _serverWrapper;
        private readonly INearestNeighborsFinder _nearestNeighborsFinder;

        public ConfigurationEnhancer(ModelService modelService, CalculationsOptimizer calculationsOptimizer) 
        {
            _serverWrapper = new ServerWrapper(modelService, calculationsOptimizer);
            _nearestNeighborsFinder = new NearestNeighborsFinder();
            _modelSelector = new ModelSelector(_serverWrapper);
            _parameterFinder = new ParameterFinder(_serverWrapper, _nearestNeighborsFinder);
        }

        public async Task<LaunchConfiguration> GetModifiedLaunchConfiguration(LaunchConfiguration launchConfiguration)
        {
            var fullParameters = new List<ParameterValue>();
            var suitableModels = _modelSelector.GetSuitableModels(launchConfiguration.Characteristics);
            launchConfiguration.Characteristics = suitableModels as List<CharacteristicWithModel>;
            var modelIds = suitableModels.Select(x => x.Model).Distinct().ToList();

            foreach (var modelId in modelIds)
            {
                var knownParametersForCurrentModel = new List<ParameterValue>();
                foreach (var parameter in launchConfiguration.Parameters)
                {
                    if (ModelService.ParametersWithModels[parameter.Id].Models.Contains(modelId))
                    {
                        knownParametersForCurrentModel.Add(parameter);
                    }
                }

                ModelConfiguration modelConfig = await _serverWrapper.GetModelConfiguration(modelId);
                if (modelConfig != null)
                {
                    if (modelConfig.ParametersInfo.Count == knownParametersForCurrentModel.Count)
                    {
                        break;
                    }
                    //ищем полное совпадение
                    var matchingParameters = await _parameterFinder.GetMatching(knownParametersForCurrentModel, modelId);
                    if (matchingParameters != null && matchingParameters.Count > 0)
                    {
                        knownParametersForCurrentModel = matchingParameters as List<ParameterValue>;
                    }
                    //ищем соседей
                    else
                    {
                        var nearestParameters = await _parameterFinder.GetNearest(knownParametersForCurrentModel, modelId, launchConfiguration.DssSearchAccuracy);
                        if (nearestParameters != null && nearestParameters.Count > 0)
                        {
                            knownParametersForCurrentModel = nearestParameters as List<ParameterValue>;
                        }
                    }
                    //проставляем дефолты
                    if (modelConfig.ParametersInfo.Count != knownParametersForCurrentModel.Count)
                    {
                        knownParametersForCurrentModel = _parameterFinder.GetDefault(knownParametersForCurrentModel, modelConfig.ParametersInfo) as List<ParameterValue>;
                    }
                    fullParameters.AddRange(knownParametersForCurrentModel);
                }
            }

            launchConfiguration.Parameters = fullParameters;
            return launchConfiguration;
        }
    }
}
