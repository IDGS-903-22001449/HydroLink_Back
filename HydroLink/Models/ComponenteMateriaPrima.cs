using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class ComponenteMateriaPrima
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ComponenteId { get; set; }
        public Componente Componente { get; set; }

        [Required]
        public int MateriaPrimaId { get; set; }
        public MateriaPrima MateriaPrima { get; set; }

        [Required]
        public decimal CantidadNecesaria { get; set; } = 1.0m;

        public decimal FactorConversion { get; set; } = 1.0m;

        public decimal PorcentajeMerma { get; set; } = 0.0m;

        public bool EsPrincipal { get; set; } = true;

        public string Notas { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public bool Activo { get; set; } = true;

        public decimal CantidadConMerma => CantidadNecesaria * FactorConversion * (1 + PorcentajeMerma);
    }
}
