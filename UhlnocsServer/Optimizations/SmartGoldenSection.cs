namespace UhlnocsServer.Optimizations
{
    public sealed class SmartGoldenSection : OptimizationAlgorithm
    {
        public string ThroughputCharacteristic { get; set; }

        public int MaxIterations { get; set; }

        public double MaxRate { get; set; }

        public double Accuracy { get; set; }

        public SmartGoldenSection(string variableParameter,
                             string throughputCharacteristic,
                             int maxIterations,
                             double maxRate,
                             double accuracy = 0.9) : base(AlgorithmType.SmartGoldenSection, variableParameter)
        {
            ThroughputCharacteristic = throughputCharacteristic;
            MaxIterations = maxIterations;
            MaxRate = maxRate;
            Accuracy = accuracy;
        }
    }
}
