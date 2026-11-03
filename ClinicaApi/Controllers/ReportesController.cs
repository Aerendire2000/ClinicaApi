using Microsoft.AspNetCore.Mvc;
using ClinicaApi.Data.Repositories;
using ClinicaApi.DTOs;

namespace ClinicaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]          // → api/reportes
    public class ReportesController : ControllerBase
    {
        private readonly IReportesRepository _reportes;

        // Se inyecta la INTERFAZ; Program.cs decide qué clase la implementa
        public ReportesController(IReportesRepository reportes)
        {
            _reportes = reportes;
        }

        // GET api/reportes/stock-medicamento/1
        [HttpGet("stock-medicamento/{medicamentoId:int}")]
        public async Task<ActionResult<IEnumerable<StockFarmaciaDto>>> StockMedicamento(
            int medicamentoId)
        {
            return Ok(await _reportes.StockDeMedicamentoAsync(medicamentoId));
        }

        // GET api/reportes/recetas-paciente/2
        [HttpGet("recetas-paciente/{pacienteId:int}")]
        public async Task<ActionResult<IEnumerable<RecetaLineaDto>>> RecetasPaciente(int pacienteId)
        {
            return Ok(await _reportes.RecetasDePacienteAsync(pacienteId));
        }

        // GET api/reportes/emergencias-activas
        [HttpGet("emergencias-activas")]
        public async Task<ActionResult<IEnumerable<EmergenciaActivaDto>>> EmergenciasActivas()
        {
            return Ok(await _reportes.EmergenciasActivasAsync());
        }
    }
}
