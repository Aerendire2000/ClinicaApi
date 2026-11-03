using Microsoft.EntityFrameworkCore;
using ClinicaApi.Models;

namespace ClinicaApi.Data
{
    // Database-First: las tablas YA existen (las crearon los scripts SQL). Solo las "mapeamos".
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

        // Sesión 9
        public DbSet<Medicamento> Medicamentos { get; set; }
        public DbSet<Ambulancia> Ambulancias { get; set; }
        public DbSet<Farmacia> Farmacias { get; set; }
        public DbSet<InventarioFarmacia> InventarioFarmacia { get; set; }
        public DbSet<RecetaMedica> RecetasMedicas { get; set; }
        public DbSet<DetalleReceta> DetalleRecetas { get; set; }
        public DbSet<Emergencia> Emergencias { get; set; }

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

            // ---------- Sesión 9: 1 a muchos ----------
            // Sede (1) → Ambulancias (N)
            modelBuilder.Entity<Ambulancia>()
                .HasOne(a => a.Sede)
                .WithMany()
                .HasForeignKey(a => a.SedeId);

            // Sede (1) → Farmacias (N)
            modelBuilder.Entity<Farmacia>()
                .HasOne(f => f.Sede)
                .WithMany()
                .HasForeignKey(f => f.SedeId);

            // Paciente (1) → Recetas (N)  y  Médico (1) → Recetas (N)
            modelBuilder.Entity<RecetaMedica>()
                .HasOne(r => r.Paciente)
                .WithMany()
                .HasForeignKey(r => r.PacienteId);

            modelBuilder.Entity<RecetaMedica>()
                .HasOne(r => r.Medico)
                .WithMany()
                .HasForeignKey(r => r.MedicoId);

            // Paciente (1) → Emergencias (N)  y  Ambulancia (1) → Emergencias (N), FK opcional
            modelBuilder.Entity<Emergencia>()
                .HasOne(e => e.Paciente)
                .WithMany()
                .HasForeignKey(e => e.PacienteId);

            modelBuilder.Entity<Emergencia>()
                .HasOne(e => e.Ambulancia)
                .WithMany()
                .HasForeignKey(e => e.AmbulanciaId)
                .IsRequired(false);

            // ---------- Sesión 9: muchos a muchos con tabla intermedia ----------
            // Farmacias (N) ↔ Medicamentos (M) a través de InventarioFarmacia
            modelBuilder.Entity<InventarioFarmacia>()
                .HasOne(i => i.Farmacia)
                .WithMany(f => f.Inventario)
                .HasForeignKey(i => i.FarmaciaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventarioFarmacia>()
                .HasOne(i => i.Medicamento)
                .WithMany()
                .HasForeignKey(i => i.MedicamentoId);

            // RecetasMedicas (N) ↔ Medicamentos (M) a través de DetalleRecetas
            modelBuilder.Entity<DetalleReceta>()
                .HasOne(d => d.Receta)
                .WithMany(r => r.Detalles)
                .HasForeignKey(d => d.RecetaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleReceta>()
                .HasOne(d => d.Medicamento)
                .WithMany()
                .HasForeignKey(d => d.MedicamentoId);
        }
    }
}
