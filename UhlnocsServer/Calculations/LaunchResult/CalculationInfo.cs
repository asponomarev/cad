namespace UhlnocsServer.Calculations.LaunchResult
{
    // list of this class objects is a part of ModelInfo and contains metainfo about calculation made by model
    public class CalculationInfo
    {
        public string CalculationId { get; set; }

        public CalculationStatus Status { get; set; }

        public bool ReallyCalculated { get; set; }

        public int IterationIndex { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public TimeSpan? Duration { get; set; }

        public string? Message { get; set; }

        public CalculationInfo(string calculationId, CalculationStatus status, bool reallyCalculated, int iterationIndex,
                               DateTime startTime, DateTime? endTime, TimeSpan? duration, string? message)
        {
            CalculationId = calculationId;
            Status = status;
            ReallyCalculated = reallyCalculated;
            IterationIndex = iterationIndex;
            StartTime = startTime;
            EndTime = endTime;
            Duration = duration;
            Message = message;
        }
    }
}
