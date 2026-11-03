using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClinicaApi.Models
{
    // Tabla "Emergencias" (1 Paciente → N Emergencias, 1 Ambulancia → N Emergencias, opcional)
    public class Emergencia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime FechaHora { get; set; }

        [Required, MaxLength(300)]
        public string Descripcion { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Prioridad { get; set; } = "Media";     // 'Alta', 'Media', 'Baja'

        [Required, MaxLength(20)]
        public string Estado { get; set; } = "Abierta";      // 'Abierta', 'En atención', 'Cerrada'

        public int PacienteId { get; set; }

        public int? AmbulanciaId { get; set; }               // int? = la FK puede ser NULL

        [JsonIgnore] public Paciente? Paciente { get; set; }
        [JsonIgnore] public Ambulancia? Ambulancia { get; set; }
    }
}
