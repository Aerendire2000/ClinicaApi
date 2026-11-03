using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClinicaApi.Models
{
    // Tabla "Especialidades" (lado UNO: una especialidad tiene muchos médicos)
    public class Especialidad
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]   // IDENTITY(1,1): lo genera SQL Server
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Descripcion { get; set; }

        // Propiedad de navegación COLECCIÓN (sesión 8)
        [JsonIgnore]
        public List<Medico> Medicos { get; set; } = new();
    }
}
