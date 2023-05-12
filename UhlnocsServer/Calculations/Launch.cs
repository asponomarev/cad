using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using UhlnocsServer.Users;

namespace UhlnocsServer.Calculations
{
    // this class describes launches table in database
    [Table("launches")]
    public class Launch
    {
        [Column("id")]
        [Key]
        public string Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }
        public User User { get; set; }

        [Column("user_parameters")]
        public JsonDocument UserParameters { get; set; }  // json with List<string>

        [Column("user_characteristics")]
        public JsonDocument UserCharacteristics { get; set; }  // json with List<CharacteristicWithModel>

        [Column("optimization_algorithm")]
        public JsonDocument OptimizationAlgorithm { get; set; }  // json with OptimizationAlgorithm

        [Column("recalculate_existing")]
        public bool RecalculateExisting { get; set; }

        [Column("dss_search_accuracy")]
        public double? DssSearchAccuracy { get; set; }

        [Column("status")]
        public LaunchStatus Status { get; set; }

        [Column("start_time")]
        public DateTime StartTime { get; set; }

        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        [Column("duration")]
        public TimeSpan? Duration { get; set; }

        public List<Calculation> Calculations { get; } = new List<Calculation>();

        public static LaunchStatus GetLaunchStatus(Task<ModelAndAlgorithmStatuses>[] modelsTasks)
        {
            int totalTasks = modelsTasks.Length;
            int modelsNoFailed = 0;
            int modelsAllFailed = 0;
            foreach (Task<ModelAndAlgorithmStatuses> task in modelsTasks)
            {
                if (task.Result.ModelStatus == ModelStatus.FinishedNoFailed)
                {
                    ++modelsNoFailed;
                }
                else if (task.Result.ModelStatus == ModelStatus.FinishedAllFailed) { }
                {
                    ++modelsAllFailed;
                }
            }

            if (totalTasks == modelsNoFailed)
            {
                return LaunchStatus.FinishedNoFailed;
            }
            if (totalTasks == modelsAllFailed)
            {
                return LaunchStatus.FinishedAllFailed;
            }
            return LaunchStatus.FinishedSomeFailed;
        }
    }
}
