using System.Text.Json;
using UhlnocsServer.Models.Properties;

namespace UhlnocsServer.Calculations.LaunchesInfos
{
    // this class is a reply body for method CalculationService.GetLaunchesInfos
    public class ShortLaunchInfo
    {
        public string LaunchId { get; set; }

        public string LaunchName { get; set; }

        public string UserId { get; set; }

        public LaunchStatus Status { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public TimeSpan? Duration { get; set; }

        public ShortLaunchInfo(string launchId, string launchName, string userId, LaunchStatus status,
                               DateTime startTime, DateTime? endTime, TimeSpan? duration)
        {
            LaunchId = launchId;
            LaunchName = launchName;
            UserId = userId;
            Status = status;
            StartTime = startTime;
            EndTime = endTime;
            Duration = duration;
        }

        public static List<ShortLaunchInfo> ListFromJsonString(string launchesInfosJson)
        {
            return JsonSerializer.Deserialize<List<ShortLaunchInfo>>(launchesInfosJson, PropertyBase.PropertySerializerOptions);
        }

        public static string ListToJsonString(List<ShortLaunchInfo> launchesInfos)
        {
            return JsonSerializer.Serialize(launchesInfos, PropertyBase.PropertySerializerOptions);
        }
    }
}
