using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClinicaApi.Models
{
    // Tabla "Farmacias" (1 Sede → N Farmacias)
    public class Farmacia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? Telefono { get; set; }

        public int SedeId { get; set; }

        [JsonIgnore]
        public Sede? Sede { get; set; }

        // 1 Farmacia → N filas de inventario (lado "uno" de la tabla intermedia)
        [JsonIgnore]
        public ICollection<InventarioFarmacia> Inventario { get; set; }
            = new List<InventarioFarmacia>();
    }

    // Tabla intermedia "InventarioFarmacia": Farmacias <-> Medicamentos (muchos a muchos)
    public class InventarioFarmacia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int FarmaciaId { get; set; }
        public int MedicamentoId { get; set; }

        public int Stock { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        public Farmacia Farmacia { get; set; } = null!;
        public Medicamento Medicamento { get; set; } = null!;
    }
}
