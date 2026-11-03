using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaApi.Data;
using ClinicaApi.DTOs;
using ClinicaApi.Models;

namespace ClinicaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]          // → api/farmacias
    public class FarmaciasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FarmaciasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/farmacias
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var farmacias = await _context.Farmacias
                .Select(f => new
                {
                    f.Id,
                    f.Nombre,
                    f.Telefono,
                    f.SedeId,
                    Sede = f.Sede!.NombreSucursal,
                    TotalMedicamentos = f.Inventario.Count
                })
                .ToListAsync();

            return Ok(farmacias);
        }

        // GET api/farmacias/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Farmacia>> GetById(int id)
        {
            var farmacia = await _context.Farmacias.FindAsync(id);
            if (farmacia is null) return NotFound($"No existe la farmacia con Id {id}.");
            return farmacia;
        }

        // GET api/farmacias/1/inventario   (N:M: Farmacia → InventarioFarmacia → Medicamento)
        [HttpGet("{id:int}/inventario")]
        public async Task<ActionResult> GetInventario(int id)
        {
            var farmacia = await _context.Farmacias
                .Include(f => f.Inventario)                 // carga la tabla intermedia
                    .ThenInclude(i => i.Medicamento)        // y desde ella, el medicamento
                .FirstOrDefaultAsync(f => f.Id == id);

            if (farmacia is null) return NotFound($"No existe la farmacia con Id {id}.");

            return Ok(new
            {
                farmacia.Id,
                farmacia.Nombre,
                Inventario = farmacia.Inventario.Select(i => new
                {
                    i.MedicamentoId,
                    Medicamento = i.Medicamento.Nombre,
                    i.Medicamento.Presentacion,
                    i.Stock,
                    i.Precio
                })
            });
        }

        // POST api/farmacias
        [HttpPost]
        public async Task<ActionResult<Farmacia>> Create([FromBody] FarmaciaDto dto)
        {
            if (!await _context.Sedes.AnyAsync(s => s.Id == dto.SedeId))
                return BadRequest($"No existe la sede con Id {dto.SedeId}.");

            var farmacia = new Farmacia
            {
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                SedeId = dto.SedeId
            };

            _context.Farmacias.Add(farmacia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = farmacia.Id }, farmacia);
        }

        // PUT api/farmacias/1/inventario/4
        // Agrega el medicamento 4 a la farmacia 1 o, si ya está, actualiza su stock y precio
        [HttpPut("{id:int}/inventario/{medicamentoId:int}")]
        public async Task<IActionResult> GuardarInventario(
            int id, int medicamentoId, [FromBody] InventarioDto dto)
        {
            if (!await _context.Farmacias.AnyAsync(f => f.Id == id))
                return NotFound($"No existe la farmacia con Id {id}.");
            if (!await _context.Medicamentos.AnyAsync(m => m.Id == medicamentoId))
                return BadRequest($"No existe el medicamento con Id {medicamentoId}.");

            var fila = await _context.InventarioFarmacia
                .FirstOrDefaultAsync(i => i.FarmaciaId == id && i.MedicamentoId == medicamentoId);

            if (fila is null)
            {
                // No existía la relación → se crea una fila en la tabla intermedia
                _context.InventarioFarmacia.Add(new InventarioFarmacia
                {
                    FarmaciaId = id,
                    MedicamentoId = medicamentoId,
                    Stock = dto.Stock,
                    Precio = dto.Precio
                });
            }
            else
            {
                fila.Stock = dto.Stock;
                fila.Precio = dto.Precio;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/farmacias/1/inventario/4
        // Quita el medicamento del inventario (NO borra el medicamento)
        [HttpDelete("{id:int}/inventario/{medicamentoId:int}")]
        public async Task<IActionResult> QuitarDelInventario(int id, int medicamentoId)
        {
            var fila = await _context.InventarioFarmacia
                .FirstOrDefaultAsync(i => i.FarmaciaId == id && i.MedicamentoId == medicamentoId);
            if (fila is null) return NotFound("Ese medicamento no está en el inventario.");

            _context.InventarioFarmacia.Remove(fila);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT api/farmacias/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] FarmaciaDto dto)
        {
            var farmacia = await _context.Farmacias.FindAsync(id);
            if (farmacia is null) return NotFound($"No existe la farmacia con Id {id}.");

            if (!await _context.Sedes.AnyAsync(s => s.Id == dto.SedeId))
                return BadRequest($"No existe la sede con Id {dto.SedeId}.");

            farmacia.Nombre = dto.Nombre;
            farmacia.Telefono = dto.Telefono;
            farmacia.SedeId = dto.SedeId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/farmacias/1
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] FarmaciaPatchDto dto)
        {
            var farmacia = await _context.Farmacias.FindAsync(id);
            if (farmacia is null) return NotFound($"No existe la farmacia con Id {id}.");

            if (dto.SedeId is not null && !await _context.Sedes.AnyAsync(s => s.Id == dto.SedeId))
                return BadRequest($"No existe la sede con Id {dto.SedeId}.");

            if (dto.Nombre is not null) farmacia.Nombre = dto.Nombre;
            if (dto.Telefono is not null) farmacia.Telefono = dto.Telefono;
            if (dto.SedeId is not null) farmacia.SedeId = dto.SedeId.Value;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/farmacias/1   (su inventario se borra solo: ON DELETE CASCADE)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var farmacia = await _context.Farmacias.FindAsync(id);
            if (farmacia is null) return NotFound($"No existe la farmacia con Id {id}.");

            _context.Farmacias.Remove(farmacia);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
