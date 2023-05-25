using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace UhlnocsServer.Calculations
{
    // this class describes characteristics table in database
    [Table("characteristics_sets")]
    public class CharacteristicsSet : IDisposable
    {
        [Column("id")]
        [Key]
        public string Id { get; set; }

        [Column("characteristics_values")]
        public JsonDocument CharacteristicsValuesJson { get; set; }  // json with List<CharacteristicValue>

        public List<Calculation> Calculations { get; } = new List<Calculation>();

        public void Dispose()
        {
            CharacteristicsValuesJson.Dispose();
        }
    }
}
