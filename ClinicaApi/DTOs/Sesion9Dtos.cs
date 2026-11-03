using System.ComponentModel.DataAnnotations;

namespace ClinicaApi.DTOs
{
    // ---------- Medicamentos ----------
    public class MedicamentoDto
    {
        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Presentacion { get; set; } = string.Empty;
    }

    // ---------- Ambulancias ----------
    public class AmbulanciaDto
    {
        [Required, RegularExpression(@"^AMB-\d{3}$", ErrorMessage = "Formato de placa: AMB-123.")]
        public string Placa { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Modelo { get; set; } = string.Empty;

        [Required]
        [RegularExpression("Disponible|En servicio|Mantenimiento",
            ErrorMessage = "Estado no válido.")]
        public string Estado { get; set; } = "Disponible";

        [Range(1, int.MaxValue, ErrorMessage = "SedeId es obligatorio.")]
        public int SedeId { get; set; }
    }

    public class AmbulanciaPatchDto
    {
        [MaxLength(100)]
        public string? Modelo { get; set; }

        [RegularExpression("Disponible|En servicio|Mantenimiento",
            ErrorMessage = "Estado no válido.")]
        public string? Estado { get; set; }

        public int? SedeId { get; set; }
    }

    // ---------- Farmacias ----------
    public class FarmaciaDto
    {
        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? Telefono { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "SedeId es obligatorio.")]
        public int SedeId { get; set; }
    }

    public class FarmaciaPatchDto
    {
        [MaxLength(100)]
        public string? Nombre { get; set; }

        [MaxLength(15)]
        public string? Telefono { get; set; }

        public int? SedeId { get; set; }
    }

    // Body para agregar/actualizar un medicamento en el inventario de una farmacia
    public class InventarioDto
    {
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        [Range(0.01, 99999, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Precio { get; set; }
    }

    // ---------- Recetas ----------
    public class RecetaDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "PacienteId es obligatorio.")]
        public int PacienteId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MedicoId es obligatorio.")]
        public int MedicoId { get; set; }

        [MaxLength(500)]
        public string? Indicaciones { get; set; }

        [MinLength(1, ErrorMessage = "La receta debe tener al menos un medicamento.")]
        public List<DetalleRecetaDto> Detalles { get; set; } = new();
    }

    public class DetalleRecetaDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "MedicamentoId es obligatorio.")]
        public int MedicamentoId { get; set; }

        [Required, MaxLength(50)]
        public string Dosis { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Frecuencia { get; set; } = string.Empty;

        [Range(1, 365, ErrorMessage = "La duración debe estar entre 1 y 365 días.")]
        public int DuracionDias { get; set; }
    }

    public class RecetaPatchDto
    {
        [MaxLength(500)]
        public string? Indicaciones { get; set; }
    }

    // Salida: la receta completa con nombres en vez de Ids
    public class RecetaRespuestaDto
    {
        public int Id { get; set; }
        public DateTime FechaEmision { get; set; }
        public string Paciente { get; set; } = string.Empty;
        public string Medico { get; set; } = string.Empty;
        public string? Indicaciones { get; set; }
        public List<DetalleRespuestaDto> Medicamentos { get; set; } = new();
    }

    public class DetalleRespuestaDto
    {
        public string Medicamento { get; set; } = string.Empty;
        public string Dosis { get; set; } = string.Empty;
        public string Frecuencia { get; set; } = string.Empty;
        public int DuracionDias { get; set; }
    }

    // ---------- Emergencias ----------
    public class EmergenciaDto
    {
        [Required, MaxLength(300)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [RegularExpression("Alta|Media|Baja",
            ErrorMessage = "Prioridad debe ser Alta, Media o Baja.")]
        public string Prioridad { get; set; } = "Media";

        [Range(1, int.MaxValue, ErrorMessage = "PacienteId es obligatorio.")]
        public int PacienteId { get; set; }

        public int? AmbulanciaId { get; set; }
    }

    public class EmergenciaPatchDto
    {
        [RegularExpression("Alta|Media|Baja",
            ErrorMessage = "Prioridad debe ser Alta, Media o Baja.")]
        public string? Prioridad { get; set; }

        [RegularExpression("Abierta|En atención|Cerrada", ErrorMessage = "Estado no válido.")]
        public string? Estado { get; set; }

        public int? AmbulanciaId { get; set; }
    }

    // ---------- Reportes con Dapper (salida) ----------
    public class StockFarmaciaDto
    {
        public string Farmacia { get; set; } = string.Empty;
        public string Sede { get; set; } = string.Empty;
        public int Stock { get; set; }
        public decimal Precio { get; set; }
    }

    public class RecetaLineaDto
    {
        public int RecetaId { get; set; }
        public DateTime FechaEmision { get; set; }
        public string Medico { get; set; } = string.Empty;
        public string Medicamento { get; set; } = string.Empty;
        public string Dosis { get; set; } = string.Empty;
        public string Frecuencia { get; set; } = string.Empty;
        public int DuracionDias { get; set; }
    }

    public class EmergenciaActivaDto
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string Prioridad { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Paciente { get; set; } = string.Empty;
        public string? Ambulancia { get; set; }
    }
}
