using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class ProductoModular
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = "Sistema Hidropónico HydroLink";

        [Required]
        [MaxLength(20)]
        public string Version { get; set; } = "v1.0";

        [MaxLength(500)]
        public string Descripcion { get; set; }

        [Required]
        public int CapacidadPorModulo { get; set; } = 20; // plantas por módulo

        [Required]
        public decimal PrecioBaseModulo { get; set; }

        public decimal PrecioModuloAdicional { get; set; }

        [MaxLength(100)]
        public string TipoPlanta { get; set; } = "Lechugas, aromáticas, vegetales";

        public string Dimensiones { get; set; } = "100cm x 60cm x 180cm por módulo";

        public string Especificaciones { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relaciones
        public ICollection<ComponenteModulo> ComponentesBase { get; set; }
        public ICollection<ComponenteModulo> ComponentesAdicionales { get; set; }
        public ICollection<CotizacionModular> CotizacionesModulares { get; set; }
    }
}
