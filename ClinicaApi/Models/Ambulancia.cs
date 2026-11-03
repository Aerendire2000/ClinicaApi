using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClinicaApi.Models
{
    // Tabla "Ambulancias" (1 Sede → N Ambulancias)
    public class Ambulancia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(10)]
        public string Placa { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Modelo { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        // 'Disponible', 'En servicio' o 'Mantenimiento'
        public string Estado { get; set; } = "Disponible";

        public int SedeId { get; set; }

        [JsonIgnore]
        public Sede? Sede { get; set; }
    }
}
