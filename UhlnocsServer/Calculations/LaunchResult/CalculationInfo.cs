namespace UhlnocsServer.Calculations.LaunchResult
{
    public class CalculationInfo
    {
        public string CalculationId { get; set; }

        public CalculationStatus Status { get; set; }

        public bool ReallyCalculated { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public TimeSpan? Duration { get; set; }

        public string? Message { get; set; }

        public CalculationInfo(string calculationId, CalculationStatus status, bool reallyCalculated,
                               DateTime startTime, DateTime? endTime, TimeSpan? duration, string? message)
        {
            CalculationId = calculationId;
            Status = status;
            ReallyCalculated = reallyCalculated;
            StartTime = startTime;
            EndTime = endTime;
            Duration = duration;
            Message = message;
        }
    }
}
