using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaApi.Data;
using ClinicaApi.Data.Repositories;
using ClinicaApi.DTOs;
using ClinicaApi.Models;

namespace ClinicaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]          // → api/citas
    public class CitasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;          // EF Core: escrituras
        private readonly IClinicaLecturaRepository _lectura;     // Dapper: lecturas

        public CitasController(
            ApplicationDbContext context, IClinicaLecturaRepository lectura)
        {
            _context = context;
            _lectura = lectura;
        }

        // GET api/citas   o   api/citas?estado=Pendiente   (Dapper → DTO de lectura)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CitaLecturaDto>>> GetAll(string? estado)
        {
            return Ok(await _lectura.ObtenerCitasAsync(estado));
        }

        // GET api/citas/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Cita>> GetById(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita is null) return NotFound($"No existe la cita con Id {id}.");
            return cita;
        }

        // POST api/citas
        [HttpPost]
        public async Task<ActionResult<Cita>> Create([FromBody] CitaDto dto)
        {
            if (!await _context.Medicos.AnyAsync(m => m.Id == dto.MedicoId))
                return BadRequest($"No existe el médico con Id {dto.MedicoId}.");
            if (!await _context.Pacientes.AnyAsync(p => p.Id == dto.PacienteId))
                return BadRequest($"No existe el paciente con Id {dto.PacienteId}.");

            var cita = new Cita
            {
                FechaHora = dto.FechaHora,
                Estado = dto.Estado,
                MedicoId = dto.MedicoId,
                PacienteId = dto.PacienteId
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = cita.Id }, cita);
        }

        // PUT api/citas/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CitaDto dto)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita is null) return NotFound($"No existe la cita con Id {id}.");

            if (!await _context.Medicos.AnyAsync(m => m.Id == dto.MedicoId))
                return BadRequest($"No existe el médico con Id {dto.MedicoId}.");
            if (!await _context.Pacientes.AnyAsync(p => p.Id == dto.PacienteId))
                return BadRequest($"No existe el paciente con Id {dto.PacienteId}.");

            cita.FechaHora = dto.FechaHora;
            cita.Estado = dto.Estado;
            cita.MedicoId = dto.MedicoId;
            cita.PacienteId = dto.PacienteId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/citas/1   (ej. solo cambiar el estado o reprogramar la hora)
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] CitaPatchDto dto)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita is null) return NotFound($"No existe la cita con Id {id}.");

            if (dto.MedicoId is not null && !await _context.Medicos.AnyAsync(m => m.Id == dto.MedicoId))
                return BadRequest($"No existe el médico con Id {dto.MedicoId}.");

            if (dto.FechaHora is not null) cita.FechaHora = dto.FechaHora.Value;
            if (dto.Estado is not null) cita.Estado = dto.Estado;
            if (dto.MedicoId is not null) cita.MedicoId = dto.MedicoId.Value;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/citas/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita is null) return NotFound($"No existe la cita con Id {id}.");

            _context.Citas.Remove(cita);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
