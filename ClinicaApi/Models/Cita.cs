using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClinicaApi.Models
{
    // Tabla "Citas" (tiene FK hacia Medicos y Pacientes)
    public class Cita
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [Required, MaxLength(50)]
        public string Estado { get; set; } = string.Empty; // 'Pendiente', 'Atendido', 'Cancelado'

        [Required]
        public int MedicoId { get; set; }

        [Required]
        public int PacienteId { get; set; }

        [ForeignKey(nameof(MedicoId))]
        [JsonIgnore]
        public Medico? Medico { get; set; }

        [ForeignKey(nameof(PacienteId))]
        [JsonIgnore]
        public Paciente? Paciente { get; set; }
    }
}