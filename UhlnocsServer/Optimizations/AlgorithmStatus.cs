namespace UhlnocsServer.Optimizations
{
    public enum AlgorithmStatus
    {
        Undefined,
        Calculating,
        FoundSaturationPoint,
        ReachedMaxIteration,
        FirstPointIsBad,
        LastPointIsGood
    }
}
