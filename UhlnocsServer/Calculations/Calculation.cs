using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UhlnocsServer.Models;

namespace UhlnocsServer.Calculations
{
    [Table("calculations")]
    [Microsoft.EntityFrameworkCore.Index("LaunchId", IsUnique = false)]
    [Microsoft.EntityFrameworkCore.Index("ParametersHash", IsUnique = false)]
    [Microsoft.EntityFrameworkCore.Index("CharacteristicsHash", IsUnique = false)]
    [Microsoft.EntityFrameworkCore.Index("ModelId", IsUnique = false)]
    public class Calculation
    {
        [Column("launch_id")]
        [Key]
        public string LaunchId { get; set; }
        public Launch Launch { get; set; }

        [Column("model_id")]
        public string ModelId { get; set; }
        public Model Model { get; set; }

        [Column("parameters_hash")]
        public string ParametersHash { get; set; }
        public ParametersSet ParametersSet { get; set; }

        [Column("characteristics_hash")]
        public string CharacteristicsHash { get; set; }
        public CharacteristicsSet CharacteristicsSet { get; set; }

        [Column("really_calculated")]
        public bool ReallyCalculated { get; set; }

        [Column("status")]
        public CalculationStatus Status { get; set; }

        [Column("start_time")]
        public DateTime StartTime { get; set; }

        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        [Column("duration")]
        public TimeSpan? Duration { get; set; }

        [Column("message")]
        public string? Message { get; set; }
    }
}
