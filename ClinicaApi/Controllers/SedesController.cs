using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaApi.Data;
using ClinicaApi.DTOs;
using ClinicaApi.Models;

namespace ClinicaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // → api/sedes
    public class SedesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SedesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/sedes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sede>>> GetAll()
        {
            return await _context.Sedes.ToListAsync();
        }

        // GET api/sedes/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Sede>> GetById(int id)
        {
            var sede = await _context.Sedes.FindAsync(id);
            if (sede is null)
                return NotFound($"No existe la sede con Id {id}.");

            return sede;
        }

        // GET api/sedes/buscar?texto=Dolores (WHERE ... LIKE '%texto%')
        [HttpGet("buscar")]
        public async Task<ActionResult<IEnumerable<Sede>>> Buscar(string texto)
        {
            return await _context.Sedes
                .Where(s => s.NombreSucursal.Contains(texto) || s.Direccion.Contains(texto))
                .ToListAsync();
        }

        // POST api/sedes
        [HttpPost]
        public async Task<ActionResult<Sede>> Create([FromBody] SedeDto dto)
        {
            var sede = new Sede
            {
                NombreSucursal = dto.NombreSucursal,
                Direccion = dto.Direccion
            };

            _context.Sedes.Add(sede);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = sede.Id }, sede);
        }

        // PUT api/sedes/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] SedeDto dto)
        {
            var sede = await _context.Sedes.FindAsync(id);
            if (sede is null)
                return NotFound($"No existe la sede con Id {id}.");

            sede.NombreSucursal = dto.NombreSucursal;
            sede.Direccion = dto.Direccion;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/sedes/1
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] SedePatchDto dto)
        {
            var sede = await _context.Sedes.FindAsync(id);
            if (sede is null)
                return NotFound($"No existe la sede con Id {id}.");

            if (dto.NombreSucursal is not null) sede.NombreSucursal = dto.NombreSucursal;
            if (dto.Direccion is not null) sede.Direccion = dto.Direccion;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/sedes/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sede = await _context.Sedes.FindAsync(id);
            if (sede is null)
                return NotFound($"No existe la sede con Id {id}.");

            _context.Sedes.Remove(sede);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}