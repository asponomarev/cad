using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
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
