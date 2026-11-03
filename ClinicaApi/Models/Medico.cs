using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace ClinicaApi.Models
{
    // Tabla "Medicos" (tiene FK hacia Especialidades)
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
        [Required]
        public int EspecialidadId { get; set; }
        // Navegación: ignorada en JSON para evitar ciclos de referencia en las respuestas
        [ForeignKey(nameof(EspecialidadId))]
        [JsonIgnore]
        public Especialidad? Especialidad { get; set; }
    }
}