using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class CotizacionModularDetalle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CotizacionModularId { get; set; }
        public CotizacionModular CotizacionModular { get; set; }

        [Required]
        public int ComponenteId { get; set; }
        public Componente Componente { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreComponente { get; set; }

        [Required]
        [MaxLength(50)]
        public string Categoria { get; set; }

        [Required]
        public decimal CantidadBase { get; set; } 

        [Required]
        public decimal CantidadAdicional { get; set; } 

        public decimal CantidadTotal => CantidadBase + CantidadAdicional;

        [Required]
        [MaxLength(20)]
        public string UnidadMedida { get; set; }

        [Required]
        public decimal PrecioUnitario { get; set; }

        public decimal SubtotalBase => CantidadBase * PrecioUnitario;

        public decimal SubtotalAdicional => CantidadAdicional * PrecioUnitario;

        public decimal SubtotalTotal => CantidadTotal * PrecioUnitario;

        [MaxLength(20)]
        public string TipoComponente { get; set; } 

        public string Especificaciones { get; set; }

        public string NotasInstalacion { get; set; }
    }
}
