using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClinicaApi.Models
{
    // Tabla "Pacientes" (lado UNO de Citas)
    public class Paciente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string DNI { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }

        // Propiedad de navegación COLECCIÓN (sesión 8): un paciente tiene muchas citas
        [JsonIgnore]
        public List<Cita> Citas { get; set; } = new();
    }
}
