using Microsoft.EntityFrameworkCore;
using ClinicaApi.Models;

namespace ClinicaApi.Data
{
    // Database-First: las tablas YA existen (las creó el script SQL). Solo las "mapeamos".
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // El nombre de cada DbSet coincide con el nombre de la tabla en SQL Server
        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<Sede> Sedes { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Medico> Medicos { get; set; }
        public DbSet<Cita> Citas { get; set; }
    }
}