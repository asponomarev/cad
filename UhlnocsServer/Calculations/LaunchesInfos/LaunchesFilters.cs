using System.Text.Json;
using UhlnocsServer.Models.Properties;

namespace UhlnocsServer.Calculations.LaunchesInfos
{
    // this class is a request body for method CalculationService.GetLaunchesInfos
    public class LaunchesFilters
    {
        public string? LaunchNamePart { get; set; }

        public string? UserIdPart { get; set; }

        public LaunchStatus? Status { get; set; }

        public DateTime? StartTimeMin { get; set; }

        public DateTime? StartTimeMax { get; set; }

        public DateTime? EndTimeMin { get; set; }

        public DateTime? EndTimeMax { get; set; }

        public TimeSpan? DurationMin { get; set; }

        public TimeSpan? DurationMax { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public LaunchesFilters(string? launchNamePart, string? userIdPart, LaunchStatus? status,
                               DateTime? startTimeMin, DateTime? startTimeMax,
                               DateTime? endTimeMin, DateTime? endTimeMax,
                               TimeSpan? durationMin, TimeSpan? durationMax,
                               int page, int pageSize)
        {
            LaunchNamePart = launchNamePart;
            UserIdPart = userIdPart;
            Status = status;
            StartTimeMin = startTimeMin;
            StartTimeMax = startTimeMax;
            EndTimeMin = endTimeMin;
            EndTimeMax = endTimeMax;
            DurationMin = durationMin;
            DurationMax = durationMax;
            Page = page;
            PageSize = pageSize;
        }

        public static LaunchesFilters FromJsonString(string launchesFiltersJson)
        {
            return JsonSerializer.Deserialize<LaunchesFilters>(launchesFiltersJson, PropertyBase.PropertySerializerOptions);
        }

        public static string ToJsonString(LaunchesFilters launchesFilters)
        {
            return JsonSerializer.Serialize(launchesFilters, PropertyBase.PropertySerializerOptions);
        }
    }
}
