using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaApi.Data;
using ClinicaApi.DTOs;
using ClinicaApi.Models;

namespace ClinicaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]          // → api/ambulancias
    public class AmbulanciasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AmbulanciasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/ambulancias   o   api/ambulancias?estado=Disponible
        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] string? estado)
        {
            var consulta = _context.Ambulancias.AsQueryable();
            if (!string.IsNullOrWhiteSpace(estado))
                consulta = consulta.Where(a => a.Estado == estado);

            var ambulancias = await consulta
                .Select(a => new
                {
                    a.Id,
                    a.Placa,
                    a.Modelo,
                    a.Estado,
                    a.SedeId,
                    Sede = a.Sede!.NombreSucursal
                })
                .ToListAsync();

            return Ok(ambulancias);
        }

        // GET api/ambulancias/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Ambulancia>> GetById(int id)
        {
            var ambulancia = await _context.Ambulancias.FindAsync(id);
            if (ambulancia is null) return NotFound($"No existe la ambulancia con Id {id}.");
            return ambulancia;
        }

        // POST api/ambulancias
        [HttpPost]
        public async Task<ActionResult<Ambulancia>> Create([FromBody] AmbulanciaDto dto)
        {
            if (!await _context.Sedes.AnyAsync(s => s.Id == dto.SedeId))
                return BadRequest($"No existe la sede con Id {dto.SedeId}.");

            // La placa es UNIQUE en SQL: validarlo antes evita un error 500
            if (await _context.Ambulancias.AnyAsync(a => a.Placa == dto.Placa))
                return Conflict($"Ya existe una ambulancia con la placa {dto.Placa}.");

            var ambulancia = new Ambulancia
            {
                Placa = dto.Placa,
                Modelo = dto.Modelo,
                Estado = dto.Estado,
                SedeId = dto.SedeId
            };

            _context.Ambulancias.Add(ambulancia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ambulancia.Id }, ambulancia);
        }

        // PUT api/ambulancias/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] AmbulanciaDto dto)
        {
            var ambulancia = await _context.Ambulancias.FindAsync(id);
            if (ambulancia is null) return NotFound($"No existe la ambulancia con Id {id}.");

            if (!await _context.Sedes.AnyAsync(s => s.Id == dto.SedeId))
                return BadRequest($"No existe la sede con Id {dto.SedeId}.");

            if (await _context.Ambulancias.AnyAsync(a => a.Placa == dto.Placa && a.Id != id))
                return Conflict($"Ya existe otra ambulancia con la placa {dto.Placa}.");

            ambulancia.Placa = dto.Placa;
            ambulancia.Modelo = dto.Modelo;
            ambulancia.Estado = dto.Estado;
            ambulancia.SedeId = dto.SedeId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/ambulancias/1   (ej: solo cambiar el estado a "Mantenimiento")
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] AmbulanciaPatchDto dto)
        {
            var ambulancia = await _context.Ambulancias.FindAsync(id);
            if (ambulancia is null) return NotFound($"No existe la ambulancia con Id {id}.");

            if (dto.SedeId is not null && !await _context.Sedes.AnyAsync(s => s.Id == dto.SedeId))
                return BadRequest($"No existe la sede con Id {dto.SedeId}.");

            if (dto.Modelo is not null) ambulancia.Modelo = dto.Modelo;
            if (dto.Estado is not null) ambulancia.Estado = dto.Estado;
            if (dto.SedeId is not null) ambulancia.SedeId = dto.SedeId.Value;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/ambulancias/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ambulancia = await _context.Ambulancias.FindAsync(id);
            if (ambulancia is null) return NotFound($"No existe la ambulancia con Id {id}.");

            if (await _context.Emergencias.AnyAsync(e => e.AmbulanciaId == id))
                return Conflict("No se puede eliminar: la ambulancia tiene emergencias.");

            _context.Ambulancias.Remove(ambulancia);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
