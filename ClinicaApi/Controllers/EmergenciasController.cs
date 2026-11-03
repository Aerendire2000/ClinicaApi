using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaApi.Data;
using ClinicaApi.DTOs;
using ClinicaApi.Models;

namespace ClinicaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]          // → api/emergencias
    public class EmergenciasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmergenciasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/emergencias
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var emergencias = await _context.Emergencias
                .OrderByDescending(e => e.FechaHora)
                .Select(e => new
                {
                    e.Id,
                    e.FechaHora,
                    e.Descripcion,
                    e.Prioridad,
                    e.Estado,
                    Paciente = e.Paciente!.Nombre,
                    Ambulancia = e.Ambulancia != null ? e.Ambulancia.Placa : null   // LEFT JOIN
                })
                .ToListAsync();

            return Ok(emergencias);
        }

        // GET api/emergencias/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Emergencia>> GetById(int id)
        {
            var emergencia = await _context.Emergencias.FindAsync(id);
            if (emergencia is null) return NotFound($"No existe la emergencia con Id {id}.");
            return emergencia;
        }

        // POST api/emergencias   (si viene AmbulanciaId, se despacha la ambulancia)
        [HttpPost]
        public async Task<ActionResult<Emergencia>> Create([FromBody] EmergenciaDto dto)
        {
            if (!await _context.Pacientes.AnyAsync(p => p.Id == dto.PacienteId))
                return BadRequest($"No existe el paciente con Id {dto.PacienteId}.");

            var emergencia = new Emergencia
            {
                FechaHora = DateTime.Now,
                Descripcion = dto.Descripcion,
                Prioridad = dto.Prioridad,
                Estado = "Abierta",
                PacienteId = dto.PacienteId
            };

            if (dto.AmbulanciaId is not null)
            {
                var error = await AsignarAmbulanciaAsync(emergencia, dto.AmbulanciaId.Value);
                if (error is not null) return error;
            }

            _context.Emergencias.Add(emergencia);
            // Guarda la emergencia Y el cambio de la ambulancia
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = emergencia.Id }, emergencia);
        }

        // PUT api/emergencias/1   (corrige los datos; la ambulancia se maneja con PATCH)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmergenciaDto dto)
        {
            var emergencia = await _context.Emergencias.FindAsync(id);
            if (emergencia is null) return NotFound($"No existe la emergencia con Id {id}.");

            if (!await _context.Pacientes.AnyAsync(p => p.Id == dto.PacienteId))
                return BadRequest($"No existe el paciente con Id {dto.PacienteId}.");

            if (dto.AmbulanciaId != emergencia.AmbulanciaId)
                return BadRequest("Para asignar o cambiar la ambulancia usa PATCH.");

            emergencia.Descripcion = dto.Descripcion;
            emergencia.Prioridad = dto.Prioridad;
            emergencia.PacienteId = dto.PacienteId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/emergencias/1   (asignar ambulancia, cambiar prioridad o cerrar)
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] EmergenciaPatchDto dto)
        {
            var emergencia = await _context.Emergencias.FindAsync(id);
            if (emergencia is null) return NotFound($"No existe la emergencia con Id {id}.");

            if (emergencia.Estado == "Cerrada")
                return Conflict("La emergencia ya está cerrada.");

            if (dto.Prioridad is not null) emergencia.Prioridad = dto.Prioridad;

            if (dto.AmbulanciaId is not null && dto.AmbulanciaId != emergencia.AmbulanciaId)
            {
                if (emergencia.AmbulanciaId is not null)
                    return Conflict("La emergencia ya tiene una ambulancia asignada.");

                var error = await AsignarAmbulanciaAsync(emergencia, dto.AmbulanciaId.Value);
                if (error is not null) return error;
            }

            if (dto.Estado is not null) emergencia.Estado = dto.Estado;

            // Al cerrar, la ambulancia queda libre otra vez
            if (emergencia.Estado == "Cerrada" && emergencia.AmbulanciaId is not null)
            {
                var ambulancia = await _context.Ambulancias.FindAsync(emergencia.AmbulanciaId);
                ambulancia!.Estado = "Disponible";
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/emergencias/1   (solo si está cerrada: es un registro histórico)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var emergencia = await _context.Emergencias.FindAsync(id);
            if (emergencia is null) return NotFound($"No existe la emergencia con Id {id}.");

            if (emergencia.Estado != "Cerrada")
                return Conflict("Solo se pueden eliminar emergencias cerradas.");

            _context.Emergencias.Remove(emergencia);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Regla de negocio: solo se despacha una ambulancia "Disponible"
        private async Task<ActionResult?> AsignarAmbulanciaAsync(
            Emergencia emergencia, int ambulanciaId)
        {
            var ambulancia = await _context.Ambulancias.FindAsync(ambulanciaId);
            if (ambulancia is null)
                return BadRequest($"No existe la ambulancia con Id {ambulanciaId}.");
            if (ambulancia.Estado != "Disponible")
                return Conflict(
                    $"La ambulancia {ambulancia.Placa} no está disponible ({ambulancia.Estado}).");

            ambulancia.Estado = "En servicio";
            emergencia.AmbulanciaId = ambulancia.Id;
            emergencia.Estado = "En atención";
            return null;
        }
    }
}
