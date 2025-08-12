using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class ComponenteModulo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductoModularId { get; set; }
        public ProductoModular ProductoModular { get; set; }

        [Required]
        public int ComponenteId { get; set; }
        public Componente Componente { get; set; }

        [Required]
        public decimal CantidadBase { get; set; } // Cantidad para el primer módulo

        [Required]
        public decimal CantidadPorModuloAdicional { get; set; } // Cantidad por cada módulo adicional

        [Required]
        [MaxLength(20)]
        public string TipoComponente { get; set; } // "BASE" o "ADICIONAL"

        [MaxLength(500)]
        public string NotasInstalacion { get; set; }

        public bool EsObligatorio { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
