namespace UhlnocsServer.Optimizations
{
    public sealed class SmartBinarySearch : OptimizationAlgorithm
    {
        public string ThroughputCharacteristic { get; set; }

        public int MaxIterations { get; set; }

        public double MaxRate { get; set; }

        public double Accuracy { get; set; }

        public SmartBinarySearch (string variableParameter,
                             string throughputCharacteristic,
                             int maxIterations,
                             double maxRate,
                             double accuracy) : base(AlgorithmType.SmartBinarySearch, variableParameter)
        {
            ThroughputCharacteristic = throughputCharacteristic;
            MaxIterations = maxIterations;
            MaxRate = maxRate;
            Accuracy = accuracy;
        }
    }
}
