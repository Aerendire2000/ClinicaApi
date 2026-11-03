using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaApi.Data;
using ClinicaApi.DTOs;
using ClinicaApi.Models;

namespace ClinicaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]          // → api/medicamentos
    public class MedicamentosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicamentosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/medicamentos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Medicamento>>> GetAll()
        {
            return await _context.Medicamentos.ToListAsync();
        }

        // GET api/medicamentos/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Medicamento>> GetById(int id)
        {
            var medicamento = await _context.Medicamentos.FindAsync(id);
            if (medicamento is null) return NotFound($"No existe el medicamento con Id {id}.");
            return medicamento;
        }

        // POST api/medicamentos
        [HttpPost]
        public async Task<ActionResult<Medicamento>> Create([FromBody] MedicamentoDto dto)
        {
            var medicamento = new Medicamento
            {
                Nombre = dto.Nombre,
                Presentacion = dto.Presentacion
            };

            _context.Medicamentos.Add(medicamento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = medicamento.Id }, medicamento);
        }

        // PUT api/medicamentos/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] MedicamentoDto dto)
        {
            var medicamento = await _context.Medicamentos.FindAsync(id);
            if (medicamento is null) return NotFound($"No existe el medicamento con Id {id}.");

            medicamento.Nombre = dto.Nombre;
            medicamento.Presentacion = dto.Presentacion;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/medicamentos/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var medicamento = await _context.Medicamentos.FindAsync(id);
            if (medicamento is null) return NotFound($"No existe el medicamento con Id {id}.");

            // Está en las dos tablas intermedias: si se usa, no se puede borrar
            if (await _context.InventarioFarmacia.AnyAsync(i => i.MedicamentoId == id) ||
                await _context.DetalleRecetas.AnyAsync(d => d.MedicamentoId == id))
                return Conflict("No se puede eliminar: el medicamento está en uso.");

            _context.Medicamentos.Remove(medicamento);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
