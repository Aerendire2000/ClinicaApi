namespace ClinicaApi.DTOs
{
    // DTOs de SALIDA (lectura): lo que devuelve la API en los GET.
    // Dapper llena cada propiedad con la columna del SELECT que tenga el mismo nombre.

    public class EspecialidadLecturaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int CantidadMedicos { get; set; }        // dato calculado con COUNT
    }

    public class PacienteLecturaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        // FechaRegistro NO se expone: es un dato interno
    }

    public class MedicoLecturaDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;   // Nombre + Apellidos
        public string NumeroColegiatura { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;     // nombre, no el Id
    }

    public class CitaLecturaDto
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Paciente { get; set; } = string.Empty;
        public string Medico { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
    }
}
