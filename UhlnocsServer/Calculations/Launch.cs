﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using UhlnocsServer.Users;

namespace UhlnocsServer.Calculations
{
    [Table("launches")]
    public class Launch
    {
        [Column("id")]
        [Key]
        public string Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }
        public User User { get; set; }

        [Column("user_parameters")]
        public JsonDocument UserParameters { get; set; }  // json with List<string>

        [Column("user_characteristics")]
        public JsonDocument UserCharacteristics { get; set; }  // json with List<string>

        [Column("optimization_algorithm")]
        public JsonDocument OptimizationAlgorithm { get; set; }  // json with OptimizationAlgorithm

        [Column("recalculate_existing")]
        public bool RecalculateExisting { get; set; }

        [Column("search_accuracy")]
        public double SearchAccuracy { get; set; }

        [Column("status")]
        public LaunchStatus Status { get; set; }

        [Column("start_time")]
        public DateTime StartTime { get; set; }

        [Column("end_time")]
        public DateTime EndTime { get; set; }

        [Column("duration")]
        public TimeSpan Duration { get; set; }

        public List<Calculation> Calculations { get; } = new List<Calculation>();
    }
}
