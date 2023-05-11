using System.Text.Json;
using UhlnocsServer.Models.Properties;

namespace UhlnocsServer.Calculations.LaunchResult
{
    public class LaunchResult
    {
        FullLaunchInfo FullLaunchInfo { get; set; }

        OptimizationAlgorithmInfo OptimizationAlgorithmInfo { get; set; }

        List<ModelInfo> ModelsInfos { get; set; }

        List<ParameterResult> ParametersResults { get; set; }

        List<CharacteristicResult> CharacteristicsResults { get; set; }

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
