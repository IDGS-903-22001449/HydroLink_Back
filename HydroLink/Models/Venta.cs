using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }
        
        [Required]
        public int ProductoId { get; set; }
        
        [Required]
        public DateTime Fecha { get; set; }
        
        [Required]
        public int Cantidad { get; set; }
        
        [Required]
        public decimal PrecioUnitario { get; set; }

        [Required]
        public decimal Total { get; set; }
        
        [StringLength(50)]
        public string Estado { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Observaciones { get; set; } = string.Empty;

        public int? CotizacionId { get; set; }
        public virtual Cotizacion? Cotizacion { get; set; }
        
        public virtual Persona Cliente { get; set; } = null!;
        public virtual ProductoHydroLink Producto { get; set; } = null!;
    }
}
