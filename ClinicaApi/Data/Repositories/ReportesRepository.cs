using Dapper;
using Microsoft.Data.SqlClient;
using ClinicaApi.DTOs;

namespace ClinicaApi.Data.Repositories
{
    // Contrato: qué consultas rápidas ofrece el repositorio
    public interface IReportesRepository
    {
        Task<IEnumerable<StockFarmaciaDto>> StockDeMedicamentoAsync(int medicamentoId);
        Task<IEnumerable<RecetaLineaDto>> RecetasDePacienteAsync(int pacienteId);
        Task<IEnumerable<EmergenciaActivaDto>> EmergenciasActivasAsync();
    }

    // Implementación con Dapper: SQL escrito a mano, mapeado directo a DTOs
    public class ReportesRepository : IReportesRepository
    {
        private readonly string _connectionString;

        public ReportesRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ConexionSQL")!;
        }

        // ¿En qué farmacias hay stock de un medicamento? (N:M Farmacias ↔ Medicamentos)
        public async Task<IEnumerable<StockFarmaciaDto>> StockDeMedicamentoAsync(int medicamentoId)
        {
            const string sql = @"
                SELECT f.Nombre AS Farmacia, s.NombreSucursal AS Sede, i.Stock, i.Precio
                FROM InventarioFarmacia i
                INNER JOIN Farmacias f ON f.Id = i.FarmaciaId
                INNER JOIN Sedes s     ON s.Id = f.SedeId
                WHERE i.MedicamentoId = @MedicamentoId AND i.Stock > 0
                ORDER BY i.Precio";

            using var conexion = new SqlConnection(_connectionString);
            return await conexion.QueryAsync<StockFarmaciaDto>(
                sql, new { MedicamentoId = medicamentoId });
        }

        // Recetas de un paciente, una fila por medicamento (N:M Recetas ↔ Medicamentos)
        public async Task<IEnumerable<RecetaLineaDto>> RecetasDePacienteAsync(int pacienteId)
        {
            const string sql = @"
                SELECT r.Id AS RecetaId, r.FechaEmision,
                       m.Nombre + ' ' + m.Apellidos AS Medico,
                       med.Nombre AS Medicamento, d.Dosis, d.Frecuencia, d.DuracionDias
                FROM RecetasMedicas r
                INNER JOIN Medicos m        ON m.Id = r.MedicoId
                INNER JOIN DetalleRecetas d ON d.RecetaId = r.Id
                INNER JOIN Medicamentos med ON med.Id = d.MedicamentoId
                WHERE r.PacienteId = @PacienteId
                ORDER BY r.FechaEmision DESC";

            using var conexion = new SqlConnection(_connectionString);
            return await conexion.QueryAsync<RecetaLineaDto>(sql, new { PacienteId = pacienteId });
        }

        // Emergencias no cerradas, las de prioridad Alta primero
        public async Task<IEnumerable<EmergenciaActivaDto>> EmergenciasActivasAsync()
        {
            const string sql = @"
                SELECT e.Id, e.FechaHora, e.Prioridad, e.Estado,
                       p.Nombre AS Paciente, a.Placa AS Ambulancia
                FROM Emergencias e
                INNER JOIN Pacientes p  ON p.Id = e.PacienteId
                LEFT JOIN Ambulancias a ON a.Id = e.AmbulanciaId
                WHERE e.Estado <> 'Cerrada'
                ORDER BY CASE e.Prioridad WHEN 'Alta' THEN 1 WHEN 'Media' THEN 2 ELSE 3 END,
                            e.FechaHora";

            using var conexion = new SqlConnection(_connectionString);
            return await conexion.QueryAsync<EmergenciaActivaDto>(sql);
        }
    }
}
