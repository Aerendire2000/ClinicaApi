using System.ComponentModel.DataAnnotations;

namespace ClinicaApi.DTOs
{
    // DTOs de entrada: lo que el cliente envía en el body
    // XxxDto      → para POST y PUT (campos obligatorios)
    // XxxPatchDto → para PATCH (todo opcional: solo se cambia lo que llega)

    // ---------- Especialidades ----------
    public class EspecialidadDto
    {
        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Descripcion { get; set; }
    }

    public class EspecialidadPatchDto
    {
        [MaxLength(100)]
        public string? Nombre { get; set; }

        [MaxLength(255)]
        public string? Descripcion { get; set; }
    }

    // ---------- Sedes ----------
    public class SedeDto
    {
        [Required, MaxLength(100)]
        public string NombreSucursal { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Direccion { get; set; } = string.Empty;
    }

    public class SedePatchDto
    {
        [MaxLength(100)]
        public string? NombreSucursal { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }
    }

    // ---------- Pacientes ----------
    public class PacienteDto
    {
        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El DNI debe tener 8 números exactos.")]
        public string DNI { get; set; } = string.Empty;
    }

    public class PacientePatchDto
    {
        [MaxLength(100)]
        public string? Nombre { get; set; }

        [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El DNI debe tener 8 números exactos.")]
        public string? DNI { get; set; }
    }

    // ---------- Médicos ----------
    public class MedicoDto
    {
        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Apellidos { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string NumeroColegiatura { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "EspecialidadId es obligatorio.")]
        public int EspecialidadId { get; set; }
    }

    public class MedicoPatchDto
    {
        [MaxLength(100)]
        public string? Nombre { get; set; }

        [MaxLength(100)]
        public string? Apellidos { get; set; }

        [MaxLength(50)]
        public string? NumeroColegiatura { get; set; }

        public int? EspecialidadId { get; set; }
    }

    // ---------- Citas ----------
    public class CitaDto
    {
        [Required]
        public DateTime FechaHora { get; set; }

        [Required, RegularExpression("Pendiente|Atendido|Cancelado", ErrorMessage = "Estado debe ser Pendiente, Atendido o Cancelado.")]
        public string Estado { get; set; } = "Pendiente";

        [Range(1, int.MaxValue, ErrorMessage = "MedicoId es obligatorio.")]
        public int MedicoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "PacienteId es obligatorio.")]
        public int PacienteId { get; set; }
    }

    public class CitaPatchDto
    {
        public DateTime? FechaHora { get; set; }

        [RegularExpression("Pendiente|Atendido|Cancelado", ErrorMessage = "Estado debe ser Pendiente, Atendido o Cancelado.")]
        public string? Estado { get; set; }

        public int? MedicoId { get; set; }
    }
}
