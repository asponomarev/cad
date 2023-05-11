﻿namespace UhlnocsServer.Calculations.LaunchResult
{
    public class FullLaunchInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public string UserId { get; set; }

        public bool RecalculateExisting { get; set; }

        public double? DssSearchAccuracy { get; set; }

        public LaunchStatus Status { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public TimeSpan? Duration { get; set; }

        public FullLaunchInfo(string id, string name, string? description, string userId,
                              bool recalculateExisting, double? dssSearchAccuracy,
                              LaunchStatus status, DateTime startTime, DateTime? endTime, TimeSpan? duration)
        {
            Id = id;
            Name = name;
            Description = description;
            UserId = userId;
            RecalculateExisting = recalculateExisting;
            DssSearchAccuracy = dssSearchAccuracy;
            Status = status;
            StartTime = startTime;
            EndTime = endTime;
            Duration = duration;
        }
    }
}