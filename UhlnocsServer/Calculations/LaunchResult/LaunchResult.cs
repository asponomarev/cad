using System.Text.Json;
using UhlnocsServer.Models.Properties;

namespace UhlnocsServer.Calculations.LaunchResult
{
    // this class is a reply body for method CalculationService.GetLaunchResult
    public class LaunchResult
    {
        public FullLaunchInfo FullLaunchInfo { get; set; }

        public OptimizationAlgorithmInfo OptimizationAlgorithmInfo { get; set; }

        public List<ModelInfo> ModelsInfos { get; set; }

        public List<ParameterResult> ParametersResults { get; set; }

        public List<CharacteristicResult> CharacteristicsResults { get; set; }

        public LaunchResult(FullLaunchInfo fullLaunchInfo, OptimizationAlgorithmInfo optimizationAlgorithmInfo,
                            List<ModelInfo> modelsInfos, List<ParameterResult> parametersResults,
                            List<CharacteristicResult> characteristicsResults)
        {
            FullLaunchInfo = fullLaunchInfo;
            OptimizationAlgorithmInfo = optimizationAlgorithmInfo;
            ModelsInfos = modelsInfos;
            ParametersResults = parametersResults;
            CharacteristicsResults = characteristicsResults;
        }

        public static LaunchResult FromJsonString(string launchResultJson)
        {
            return JsonSerializer.Deserialize<LaunchResult>(launchResultJson, PropertyBase.PropertySerializerOptions);
        }

        public static string ToJsonString(LaunchResult launchResult)
        {
            return JsonSerializer.Serialize(launchResult, PropertyBase.PropertySerializerOptions);
        }
    }
}
