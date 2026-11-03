using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaApi.Data;
using ClinicaApi.DTOs;
using ClinicaApi.Models;

namespace ClinicaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // → api/medicos
    public class MedicosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/medicos (Medicos JOIN Especialidades: devuelve el nombre de la especialidad)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var medicos = await _context.Medicos
                .Select(m => new
                {
                    m.Id,
                    m.Nombre,
                    m.Apellidos,
                    m.NumeroColegiatura,
                    m.EspecialidadId,
                    Especialidad = m.Especialidad!.Nombre
                })
                .ToListAsync();

            return Ok(medicos);
        }

        // GET api/medicos/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Medico>> GetById(int id)
        {
            var medico = await _context.Medicos.FindAsync(id);
            if (medico is null)
                return NotFound($"No existe el médico con Id {id}.");

            return medico;
        }

        // POST api/medicos
        [HttpPost]
        public async Task<ActionResult<Medico>> Create([FromBody] MedicoDto dto)
        {
            // Validar la FK antes de guardar (si no, SQL Server lanza error 500)
            if (!await _context.Especialidades.AnyAsync(e => e.Id == dto.EspecialidadId))
                return BadRequest($"No existe la especialidad con Id {dto.EspecialidadId}.");

            var medico = new Medico
            {
                Nombre = dto.Nombre,
                Apellidos = dto.Apellidos,
                NumeroColegiatura = dto.NumeroColegiatura,
                EspecialidadId = dto.EspecialidadId
            };

            _context.Medicos.Add(medico);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = medico.Id }, medico);
        }

        // PUT api/medicos/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] MedicoDto dto)
        {
            var medico = await _context.Medicos.FindAsync(id);
            if (medico is null)
                return NotFound($"No existe el médico con Id {id}.");

            if (!await _context.Especialidades.AnyAsync(e => e.Id == dto.EspecialidadId))
                return BadRequest($"No existe la especialidad con Id {dto.EspecialidadId}.");

            medico.Nombre = dto.Nombre;
            medico.Apellidos = dto.Apellidos;
            medico.NumeroColegiatura = dto.NumeroColegiatura;
            medico.EspecialidadId = dto.EspecialidadId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/medicos/1
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] MedicoPatchDto dto)
        {
            var medico = await _context.Medicos.FindAsync(id);
            if (medico is null)
                return NotFound($"No existe el médico con Id {id}.");

            if (dto.EspecialidadId is not null &&
                !await _context.Especialidades.AnyAsync(e => e.Id == dto.EspecialidadId))
                return BadRequest($"No existe la especialidad con Id {dto.EspecialidadId}.");

            if (dto.Nombre is not null) medico.Nombre = dto.Nombre;
            if (dto.Apellidos is not null) medico.Apellidos = dto.Apellidos;
            if (dto.NumeroColegiatura is not null) medico.NumeroColegiatura = dto.NumeroColegiatura;
            if (dto.EspecialidadId is not null) medico.EspecialidadId = dto.EspecialidadId.Value;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/medicos/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var medico = await _context.Medicos.FindAsync(id);
            if (medico is null)
                return NotFound($"No existe el médico con Id {id}.");

            if (await _context.Citas.AnyAsync(c => c.MedicoId == id))
                return Conflict("No se puede eliminar: el médico tiene citas registradas.");

            _context.Medicos.Remove(medico);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}