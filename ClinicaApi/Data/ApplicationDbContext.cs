using Microsoft.EntityFrameworkCore;
using ClinicaApi.Models;

namespace ClinicaApi.Data
{
    // Database-First: las tablas YA existen (las creó el script SQL). Solo las "mapeamos".
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // El nombre de cada DbSet coincide con el nombre de la tabla en SQL Server
        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<Sede> Sedes { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Medico> Medicos { get; set; }
        public DbSet<Cita> Citas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---------- Sesión 8 ----------
            // 1:N  Especialidad (1) → Médicos (N)
            modelBuilder.Entity<Medico>()                    // empezamos por el hijo
                .HasOne(m => m.Especialidad)                 // un médico tiene UNA especialidad
                .WithMany(e => e.Medicos)                    // y la especialidad tiene MUCHOS
                .HasForeignKey(m => m.EspecialidadId)        // la llave foránea es EspecialidadId
                .OnDelete(DeleteBehavior.Restrict);          // no se borra si tiene médicos

            // N:M  Médicos ↔ Pacientes a través de la tabla intermedia Citas
            // Relación 1: Médico (1) → Citas (N)
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Medico)
                .WithMany(m => m.Citas)
                .HasForeignKey(c => c.MedicoId);

            // Relación 2: Paciente (1) → Citas (N)
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Paciente)
                .WithMany(p => p.Citas)
                .HasForeignKey(c => c.PacienteId);
        }
    }
}
