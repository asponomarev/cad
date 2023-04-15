namespace UhlnocsServer.Optimizations
{
    public sealed class SmartConstantStep : OptimizationAlgorithm
    {
        public string ThroughputCharacteristic { get; set; }

        public double Step { get; set; }

        public int MaxIterations { get; set; }

        public double Accuracy { get; set; }

        public SmartConstantStep (string variableParameter,
                                  string throughputCharacteristic,
                                  double step,
                                  int maxIterations,
                                  double accuracy) : base(AlgorithmType.SmartConstantStep, variableParameter)
        {
            ThroughputCharacteristic = throughputCharacteristic;
            Step = step;
            MaxIterations = maxIterations;
            Accuracy = accuracy;
        }
    }
}
