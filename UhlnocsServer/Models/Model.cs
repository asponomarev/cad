using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using UhlnocsServer.Calculations;

namespace UhlnocsServer.Models
{
    // this class describes models table in database
    [Table("models")]
    public class Model : IDisposable
    {
        [Column("id")]
        [Key]
        public string Id { get; set; }

        [Column("configuration")]
        public JsonDocument Configuration { get; set; }

        [Column("performance")]
        public double Performance { get; set; }

        public List<Calculation> Calculations { get; } = new List<Calculation>();

        public Model(string id, JsonDocument configuration)
        {
            Id = id;
            Configuration = configuration;
            Performance = default;  // 0
        }

        public void Dispose()
        {
            Configuration.Dispose();
        }
    }
}
