using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClinicaApi.Models
{
    // Tabla "RecetasMedicas" (1 Paciente → N Recetas, 1 Médico → N Recetas)
    public class RecetaMedica
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime FechaEmision { get; set; }

        [MaxLength(500)]
        public string? Indicaciones { get; set; }

        public int PacienteId { get; set; }
        public int MedicoId { get; set; }

        [JsonIgnore] public Paciente? Paciente { get; set; }
        [JsonIgnore] public Medico? Medico { get; set; }

        // 1 Receta → N detalles (cada detalle es un medicamento recetado)
        [JsonIgnore]
        public ICollection<DetalleReceta> Detalles { get; set; } = new List<DetalleReceta>();
    }

    // Tabla intermedia "DetalleRecetas": RecetasMedicas <-> Medicamentos (muchos a muchos)
    public class DetalleReceta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int RecetaId { get; set; }
        public int MedicamentoId { get; set; }

        [Required, MaxLength(50)]
        public string Dosis { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Frecuencia { get; set; } = string.Empty;

        public int DuracionDias { get; set; }

        public RecetaMedica Receta { get; set; } = null!;
        public Medicamento Medicamento { get; set; } = null!;
    }
}
