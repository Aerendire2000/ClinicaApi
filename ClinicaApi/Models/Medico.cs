using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClinicaApi.Models
{
    // Tabla "Medicos" (lado MUCHOS de Especialidad y lado UNO de Citas)
    public class Medico
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Apellidos { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string NumeroColegiatura { get; set; } = string.Empty;

        // Llave foránea explícita
        [Required]
        public int EspecialidadId { get; set; }

        // Propiedad de navegación REFERENCIA: ignorada en JSON para evitar ciclos
        [ForeignKey(nameof(EspecialidadId))]
        [JsonIgnore]
        public Especialidad? Especialidad { get; set; }

        // Propiedad de navegación COLECCIÓN (sesión 8): un médico tiene muchas citas
        [JsonIgnore]
        public List<Cita> Citas { get; set; } = new();
    }
}
