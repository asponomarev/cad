using UhlnocsServer.Optimizations;

namespace UhlnocsServer.Calculations
{
    public class ModelAndAlgorithmStatuses
    {
        public string ModelId { get; set; }

        public ModelStatus ModelStatus { get; set; }

        public AlgorithmStatus AlgorithmStatus { get; set; }

        public ModelAndAlgorithmStatuses(string modelId, ModelStatus modelStatus, AlgorithmStatus algorithmStatus) 
        {
            ModelId = modelId;
            ModelStatus = modelStatus;
            AlgorithmStatus = algorithmStatus;           
        }
    }
}
