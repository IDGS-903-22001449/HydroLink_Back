using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class Cotizacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreProyecto { get; set; }

        [MaxLength(500)]
        public string Descripcion { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreCliente { get; set; }

        [MaxLength(100)]
        public string EmailCliente { get; set; }

        [MaxLength(20)]
        public string TelefonoCliente { get; set; }

        public int? UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        // Nuevas propiedades para el sistema simplificado
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;
        
        public int ProductoId { get; set; }
        public virtual ProductoHydroLink Producto { get; set; } = null!;

        public ICollection<CotizacionDetalle> Detalles { get; set; }

        [Required]
        public decimal SubtotalComponentes { get; set; }

        [Required]
        public decimal SubtotalManoObra { get; set; }

        [Required]
        public decimal SubtotalMateriales { get; set; }

        [Required]
        public decimal TotalEstimado { get; set; }

        public decimal PorcentajeGanancia { get; set; } = 20.0m; // 20% por defecto

        public decimal MontoGanancia { get; set; }

        [MaxLength(20)]
        public string Estado { get; set; } = "BORRADOR"; // BORRADOR, ENVIADA, APROBADA, RECHAZADA

        public DateTime? FechaVencimiento { get; set; }

        public string Observaciones { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }
        
        // Relación con Venta (una cotización puede generar una venta)
        public int? VentaId { get; set; }
        public virtual Venta? Venta { get; set; }
    }
}
