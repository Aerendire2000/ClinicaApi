using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaApi.Data;
using ClinicaApi.DTOs;
using ClinicaApi.Models;

namespace ClinicaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]          // → api/recetas
    public class RecetasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RecetasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/recetas   (resumen: paciente, médico y cuántos medicamentos tiene)
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var recetas = await _context.RecetasMedicas
                .Select(r => new
                {
                    r.Id,
                    r.FechaEmision,
                    Paciente = r.Paciente!.Nombre,
                    Medico = r.Medico!.Nombre + " " + r.Medico.Apellidos,
                    CantidadMedicamentos = r.Detalles.Count
                })
                .ToListAsync();

            return Ok(recetas);
        }

        // GET api/recetas/1   (receta completa: Include + ThenInclude)
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RecetaRespuestaDto>> GetById(int id)
        {
            var receta = await _context.RecetasMedicas
                .Include(r => r.Paciente)
                .Include(r => r.Medico)
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.Medicamento)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receta is null) return NotFound($"No existe la receta con Id {id}.");

            return new RecetaRespuestaDto
            {
                Id = receta.Id,
                FechaEmision = receta.FechaEmision,
                Paciente = receta.Paciente!.Nombre,
                Medico = receta.Medico!.Nombre + " " + receta.Medico.Apellidos,
                Indicaciones = receta.Indicaciones,
                Medicamentos = receta.Detalles.Select(d => new DetalleRespuestaDto
                {
                    Medicamento = d.Medicamento.Nombre + " " + d.Medicamento.Presentacion,
                    Dosis = d.Dosis,
                    Frecuencia = d.Frecuencia,
                    DuracionDias = d.DuracionDias
                }).ToList()
            };
        }

        // POST api/recetas   (cabecera + detalles en una sola petición)
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] RecetaDto dto)
        {
            var error = await ValidarAsync(dto);
            if (error is not null) return BadRequest(error);

            var receta = new RecetaMedica
            {
                FechaEmision = DateTime.Now,
                Indicaciones = dto.Indicaciones,
                PacienteId = dto.PacienteId,
                MedicoId = dto.MedicoId,
                // Los detalles se agregan a la colección: EF les pone el RecetaId solo
                Detalles = dto.Detalles.Select(d => new DetalleReceta
                {
                    MedicamentoId = d.MedicamentoId,
                    Dosis = d.Dosis,
                    Frecuencia = d.Frecuencia,
                    DuracionDias = d.DuracionDias
                }).ToList()
            };

            _context.RecetasMedicas.Add(receta);
            // 1 INSERT a RecetasMedicas + N INSERT a DetalleRecetas
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = receta.Id }, new { receta.Id });
        }

        // PUT api/recetas/1   (reemplaza la receta completa, incluidos sus detalles)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] RecetaDto dto)
        {
            var receta = await _context.RecetasMedicas
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (receta is null) return NotFound($"No existe la receta con Id {id}.");

            var error = await ValidarAsync(dto);
            if (error is not null) return BadRequest(error);

            receta.PacienteId = dto.PacienteId;
            receta.MedicoId = dto.MedicoId;
            receta.Indicaciones = dto.Indicaciones;

            // Borrar los detalles viejos y poner los nuevos
            _context.DetalleRecetas.RemoveRange(receta.Detalles);
            foreach (var d in dto.Detalles)
            {
                receta.Detalles.Add(new DetalleReceta
                {
                    MedicamentoId = d.MedicamentoId,
                    Dosis = d.Dosis,
                    Frecuencia = d.Frecuencia,
                    DuracionDias = d.DuracionDias
                });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/recetas/1   (solo las indicaciones)
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] RecetaPatchDto dto)
        {
            var receta = await _context.RecetasMedicas.FindAsync(id);
            if (receta is null) return NotFound($"No existe la receta con Id {id}.");

            if (dto.Indicaciones is not null) receta.Indicaciones = dto.Indicaciones;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/recetas/1   (sus detalles se borran solos: ON DELETE CASCADE)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var receta = await _context.RecetasMedicas.FindAsync(id);
            if (receta is null) return NotFound($"No existe la receta con Id {id}.");

            _context.RecetasMedicas.Remove(receta);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Valida las FKs de la cabecera y de cada detalle. Devuelve null si todo está bien.
        private async Task<string?> ValidarAsync(RecetaDto dto)
        {
            if (!await _context.Pacientes.AnyAsync(p => p.Id == dto.PacienteId))
                return $"No existe el paciente con Id {dto.PacienteId}.";
            if (!await _context.Medicos.AnyAsync(m => m.Id == dto.MedicoId))
                return $"No existe el médico con Id {dto.MedicoId}.";

            // HashSet: Ids sin repetir → una sola consulta para verificar todos los medicamentos
            var ids = dto.Detalles.Select(d => d.MedicamentoId).ToHashSet();
            if (ids.Count != dto.Detalles.Count)
                return "Un medicamento no puede repetirse en la misma receta.";

            var existentes = await _context.Medicamentos.CountAsync(m => ids.Contains(m.Id));
            if (existentes != ids.Count)
                return "Uno o más medicamentos no existen.";

            return null;
        }
    }
}
