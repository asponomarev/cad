namespace UhlnocsServer.Optimizations
{
    public sealed class ConstantStep : OptimizationAlgorithm
    {
        public double Step { get; set; }

        public int Iterations { get; set; }

        public ConstantStep(string variableParameter, double step, int iterations) : base(AlgorithmType.ConstantStep, variableParameter)
        {
            Step = step;
            Iterations = iterations;
        }
    }
}
