using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaApi.Data;
using ClinicaApi.Data.Repositories;
using ClinicaApi.DTOs;
using ClinicaApi.Models;

namespace ClinicaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]          // → api/especialidades
    public class EspecialidadesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;          // EF Core: escrituras
        private readonly IClinicaLecturaRepository _lectura;     // Dapper: lecturas

        public EspecialidadesController(
            ApplicationDbContext context, IClinicaLecturaRepository lectura)
        {
            _context = context;
            _lectura = lectura;
        }

        // GET api/especialidades   (Dapper → DTO de lectura, con cantidad de médicos)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EspecialidadLecturaDto>>> GetAll()
        {
            return Ok(await _lectura.ObtenerEspecialidadesAsync());
        }

        // GET api/especialidades/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Especialidad>> GetById(int id)
        {
            var especialidad = await _context.Especialidades.FindAsync(id);
            if (especialidad is null) return NotFound($"No existe la especialidad con Id {id}.");
            return especialidad;
        }

        // GET api/especialidades/1/medicos   (Especialidades JOIN Medicos)
        [HttpGet("{id:int}/medicos")]
        public async Task<ActionResult> GetMedicos(int id)
        {
            // Usa la colección de navegación e.Medicos (sesión 8) con Include
            var especialidad = await _context.Especialidades
                .Include(e => e.Medicos)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (especialidad is null) return NotFound($"No existe la especialidad con Id {id}.");

            var medicos = especialidad.Medicos.Select(m => new MedicoLecturaDto
            {
                Id = m.Id,
                NombreCompleto = $"{m.Nombre} {m.Apellidos}",
                NumeroColegiatura = m.NumeroColegiatura,
                Especialidad = especialidad.Nombre
            });

            return Ok(medicos);
        }

        // POST api/especialidades
        [HttpPost]
        public async Task<ActionResult<Especialidad>> Create([FromBody] EspecialidadDto dto)
        {
            var especialidad = new Especialidad { Nombre = dto.Nombre, Descripcion = dto.Descripcion };

            _context.Especialidades.Add(especialidad);
            await _context.SaveChangesAsync();          // aquí SQL Server genera el Id

            return CreatedAtAction(nameof(GetById), new { id = especialidad.Id }, especialidad);
        }

        // PUT api/especialidades/1   (reemplaza todos los campos)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] EspecialidadDto dto)
        {
            var especialidad = await _context.Especialidades.FindAsync(id);
            if (especialidad is null) return NotFound($"No existe la especialidad con Id {id}.");

            especialidad.Nombre = dto.Nombre;
            especialidad.Descripcion = dto.Descripcion;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/especialidades/1   (solo los campos enviados)
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] EspecialidadPatchDto dto)
        {
            var especialidad = await _context.Especialidades.FindAsync(id);
            if (especialidad is null) return NotFound($"No existe la especialidad con Id {id}.");

            if (dto.Nombre is not null) especialidad.Nombre = dto.Nombre;
            if (dto.Descripcion is not null) especialidad.Descripcion = dto.Descripcion;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/especialidades/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var especialidad = await _context.Especialidades.FindAsync(id);
            if (especialidad is null) return NotFound($"No existe la especialidad con Id {id}.");

            // La FK FK_Medicos_Especialidades no deja borrar si hay médicos con esta especialidad
            if (await _context.Medicos.AnyAsync(m => m.EspecialidadId == id))
                return Conflict("No se puede eliminar: hay médicos con esta especialidad.");

            _context.Especialidades.Remove(especialidad);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
