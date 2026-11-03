using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaApi.Data;
using ClinicaApi.Data.Repositories;
using ClinicaApi.DTOs;
using ClinicaApi.Models;

namespace ClinicaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]          // → api/pacientes
    public class PacientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;          // EF Core: escrituras
        private readonly IClinicaLecturaRepository _lectura;     // Dapper: lecturas

        public PacientesController(
            ApplicationDbContext context, IClinicaLecturaRepository lectura)
        {
            _context = context;
            _lectura = lectura;
        }

        // GET api/pacientes   (Dapper → DTO de lectura, sin FechaRegistro)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PacienteLecturaDto>>> GetAll()
        {
            return Ok(await _lectura.ObtenerPacientesAsync());
        }

        // GET api/pacientes/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Paciente>> GetById(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);
            if (paciente is null) return NotFound($"No existe el paciente con Id {id}.");
            return paciente;
        }

        // GET api/pacientes/dni/72345678
        [HttpGet("dni/{dni}")]
        public async Task<ActionResult<Paciente>> GetByDni(string dni)
        {
            var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.DNI == dni);
            if (paciente is null) return NotFound($"No existe un paciente con DNI {dni}.");
            return paciente;
        }

        // GET api/pacientes/1/citas   (Citas JOIN Medicos JOIN Especialidades)
        [HttpGet("{id:int}/citas")]
        public async Task<ActionResult> GetCitas(int id)
        {
            if (!await _context.Pacientes.AnyAsync(p => p.Id == id))
                return NotFound($"No existe el paciente con Id {id}.");

            var citas = await _context.Citas
                .Where(c => c.PacienteId == id)
                .OrderBy(c => c.FechaHora)
                .Select(c => new
                {
                    c.Id,
                    c.FechaHora,
                    c.Estado,
                    Medico = c.Medico!.Nombre + " " + c.Medico.Apellidos,
                    Especialidad = c.Medico.Especialidad!.Nombre
                })
                .ToListAsync();

            return Ok(citas);
        }

        // POST api/pacientes
        [HttpPost]
        public async Task<ActionResult<Paciente>> Create([FromBody] PacienteDto dto)
        {
            if (await _context.Pacientes.AnyAsync(p => p.DNI == dto.DNI))
                return Conflict($"Ya existe un paciente con DNI {dto.DNI}.");

            var paciente = new Paciente
            {
                Nombre = dto.Nombre,
                DNI = dto.DNI,
                FechaRegistro = DateTime.Now      // la fecha la pone el servidor, no el cliente
            };

            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = paciente.Id }, paciente);
        }

        // PUT api/pacientes/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PacienteDto dto)
        {
            var paciente = await _context.Pacientes.FindAsync(id);
            if (paciente is null) return NotFound($"No existe el paciente con Id {id}.");

            if (await _context.Pacientes.AnyAsync(p => p.DNI == dto.DNI && p.Id != id))
                return Conflict($"Ya existe otro paciente con DNI {dto.DNI}.");

            paciente.Nombre = dto.Nombre;
            paciente.DNI = dto.DNI;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/pacientes/1
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] PacientePatchDto dto)
        {
            var paciente = await _context.Pacientes.FindAsync(id);
            if (paciente is null) return NotFound($"No existe el paciente con Id {id}.");

            if (dto.DNI is not null && await _context.Pacientes.AnyAsync(p => p.DNI == dto.DNI && p.Id != id))
                return Conflict($"Ya existe otro paciente con DNI {dto.DNI}.");

            if (dto.Nombre is not null) paciente.Nombre = dto.Nombre;
            if (dto.DNI is not null) paciente.DNI = dto.DNI;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/pacientes/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);
            if (paciente is null) return NotFound($"No existe el paciente con Id {id}.");

            if (await _context.Citas.AnyAsync(c => c.PacienteId == id))
                return Conflict("No se puede eliminar: el paciente tiene citas registradas.");

            _context.Pacientes.Remove(paciente);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
