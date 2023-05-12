using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace UhlnocsServer.Calculations
{
    // this class describes parameters table in database
    [Table("parameters_sets")]
    public class ParametersSet : IDisposable
    {
        [Column("hash")]
        [Key]
        public string Hash { get; set; }

        [Column("parameters_values")]
        public JsonDocument ParametersValuesJson { get; set; }  // json with List<ParameterValue>

        public List<Calculation> Calculations { get; } = new List<Calculation>();

        public void Dispose()
        { 
            ParametersValuesJson.Dispose();
        }
    }
}
