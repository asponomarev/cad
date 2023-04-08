using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace UhlnocsServer.Calculations
{
    [Table("characteristics_sets")]
    public class CharacteristicsSet : IDisposable
    {
        [Column("hash")]
        [Key]
        public string Hash { get; set; }

        [Column("characteristics_values")]
        public JsonDocument CharacteristicsValuesJson { get; set; }  // json with List<CharacteristicValue>

        public void Dispose()
        {
            CharacteristicsValuesJson.Dispose();
        }
    }
}
