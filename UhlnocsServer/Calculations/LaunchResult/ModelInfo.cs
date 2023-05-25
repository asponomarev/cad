using UhlnocsServer.Optimizations;

namespace UhlnocsServer.Calculations.LaunchResult
{
    // list of this class objects is a part of LaunchResult and contains info about model used in launch
    public class ModelInfo
    {
        public string ModelId { get; set; }

        public string ModelName { get; set; }

        public AlgorithmStatus AlgorithmStatus { get; set; }

        public List<CalculationInfo> CalculationsInfos { get; set; }

        public ModelInfo(string modelId, string modelName,
                         AlgorithmStatus algorithmStatus, List<CalculationInfo> calculationsInfos)
        {
            ModelId = modelId;
            ModelName = modelName;
            AlgorithmStatus = algorithmStatus;
            CalculationsInfos = calculationsInfos;
        }



    }
}
