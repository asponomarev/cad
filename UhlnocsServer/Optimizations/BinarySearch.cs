namespace UhlnocsServer.Optimizations
{
    public sealed class BinarySearch : OptimizationAlgorithm
    {
        public string ThroughputCharacteristic { get; set; }

        public int Iterations { get; set; }

        public double MaxRate { get; set; }

        public double Accuracy { get; set; }

        public BinarySearch (string variableParameter,
                             string throughputCharacteristic,
                             int iterations,
                             double maxRate,
                             double accuracy = 0.9) : base(AlgorithmType.BinarySearch, variableParameter)
        {
            ThroughputCharacteristic = throughputCharacteristic;
            Iterations = iterations;
            MaxRate = maxRate;
            Accuracy = accuracy;
        }
    }
}
