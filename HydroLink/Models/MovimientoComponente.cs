using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class MovimientoComponente
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ComponenteId { get; set; }

        [Required]
        public DateTime FechaMovimiento { get; set; }

        [Required]
        [StringLength(20)]
        public string TipoMovimiento { get; set; } = string.Empty; // ENTRADA, SALIDA

        [Required]
        public decimal Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal CostoTotal { get; set; }

        [StringLength(50)]
        public string NumeroLote { get; set; } = string.Empty;
        
        public int? VentaId { get; set; }
        
        public int? CompraId { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; } = string.Empty;

        // Navegación
        public virtual Componente Componente { get; set; } = null!;
        public virtual Venta? Venta { get; set; }
        public virtual Compra? Compra { get; set; }
    }
}
