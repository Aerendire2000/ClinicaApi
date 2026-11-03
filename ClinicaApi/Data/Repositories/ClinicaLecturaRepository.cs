using Dapper;
using Microsoft.Data.SqlClient;
using ClinicaApi.DTOs;

namespace ClinicaApi.Data.Repositories
{
    // Contrato: las consultas de LECTURA de la clínica (CQRS Lite: los GET van por Dapper)
    public interface IClinicaLecturaRepository
    {
        Task<IEnumerable<EspecialidadLecturaDto>> ObtenerEspecialidadesAsync();
        Task<IEnumerable<PacienteLecturaDto>> ObtenerPacientesAsync();
        Task<IEnumerable<MedicoLecturaDto>> ObtenerMedicosAsync(int? especialidadId);
        Task<IEnumerable<CitaLecturaDto>> ObtenerCitasAsync(string? estado);
    }

    public class ClinicaLecturaRepository : IClinicaLecturaRepository
    {
        private readonly string _connectionString;

        public ClinicaLecturaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ConexionSQL")!;
        }

        // Especialidades con cuántos médicos tiene cada una (LEFT JOIN + COUNT)
        public async Task<IEnumerable<EspecialidadLecturaDto>> ObtenerEspecialidadesAsync()
        {
            const string sql = @"
                SELECT e.Id, e.Nombre, e.Descripcion, COUNT(m.Id) AS CantidadMedicos
                FROM Especialidades e
                LEFT JOIN Medicos m ON m.EspecialidadId = e.Id
                GROUP BY e.Id, e.Nombre, e.Descripcion
                ORDER BY e.Nombre";

            using var conexion = new SqlConnection(_connectionString);
            return await conexion.QueryAsync<EspecialidadLecturaDto>(sql);
        }

        // Pacientes sin FechaRegistro
        public async Task<IEnumerable<PacienteLecturaDto>> ObtenerPacientesAsync()
        {
            const string sql = "SELECT Id, Nombre, DNI FROM Pacientes ORDER BY Nombre";

            using var conexion = new SqlConnection(_connectionString);
            return await conexion.QueryAsync<PacienteLecturaDto>(sql);
        }

        // Médicos con el nombre de su especialidad; filtro opcional por especialidad
        public async Task<IEnumerable<MedicoLecturaDto>> ObtenerMedicosAsync(int? especialidadId)
        {
            const string sql = @"
                SELECT m.Id, m.Nombre + ' ' + m.Apellidos AS NombreCompleto,
                       m.NumeroColegiatura, e.Nombre AS Especialidad
                FROM Medicos m
                INNER JOIN Especialidades e ON e.Id = m.EspecialidadId
                WHERE (@EspecialidadId IS NULL OR m.EspecialidadId = @EspecialidadId)
                ORDER BY m.Apellidos";

            using var conexion = new SqlConnection(_connectionString);
            return await conexion.QueryAsync<MedicoLecturaDto>(
                sql, new { EspecialidadId = especialidadId });
        }

        // Citas con paciente, médico y especialidad (la tabla intermedia N:M); filtro opcional
        public async Task<IEnumerable<CitaLecturaDto>> ObtenerCitasAsync(string? estado)
        {
            const string sql = @"
                SELECT c.Id, c.FechaHora, c.Estado,
                       p.Nombre AS Paciente,
                       m.Nombre + ' ' + m.Apellidos AS Medico,
                       e.Nombre AS Especialidad
                FROM Citas c
                INNER JOIN Pacientes p      ON p.Id = c.PacienteId
                INNER JOIN Medicos m        ON m.Id = c.MedicoId
                INNER JOIN Especialidades e ON e.Id = m.EspecialidadId
                WHERE (@Estado IS NULL OR c.Estado = @Estado)
                ORDER BY c.FechaHora";

            using var conexion = new SqlConnection(_connectionString);
            return await conexion.QueryAsync<CitaLecturaDto>(sql, new { Estado = estado });
        }
    }
}
