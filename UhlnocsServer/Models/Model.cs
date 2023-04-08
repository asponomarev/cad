using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using UhlnocsServer.Calculations;

namespace UhlnocsServer.Models
{
    [Table("models")]
    public class Model : IDisposable
    {
        [Column("id")]
        [Key]
        public string Id { get; set; }

        [Column("configuration")]
        public JsonDocument Configuration { get; set; }

        public List<Calculation> Calculations { get; } = new List<Calculation>();

        public Model(string id, JsonDocument configuration)
        {
            Id = id;
            Configuration = configuration;
        }

        public void Dispose()
        {
            Configuration.Dispose();
        }
    }
}
