using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class CotizacionModular
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductoModularId { get; set; }
        public ProductoModular ProductoModular { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreCliente { get; set; }

        [MaxLength(100)]
        public string EmailCliente { get; set; }

        [MaxLength(20)]
        public string TelefonoCliente { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreProyecto { get; set; }

        [Required]
        public int CantidadModulos { get; set; }

        public int CapacidadTotalPlantas => (ProductoModular?.CapacidadPorModulo ?? 0) * CantidadModulos;

        [Required]
        public decimal SubtotalComponentes { get; set; }

        [Required]
        public decimal SubtotalManoObra { get; set; }

        [Required]
        public decimal SubtotalMateriales { get; set; }

        [Required]
        public decimal TotalEstimado { get; set; }

        public decimal PorcentajeGanancia { get; set; } = 20.0m;

        public decimal MontoGanancia { get; set; }

        [MaxLength(20)]
        public string Estado { get; set; } = "PENDIENTE"; // PENDIENTE, ENVIADA, APROBADA, RECHAZADA

        public string EspecificacionesEspeciales { get; set; }

        public string Observaciones { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public DateTime? FechaVencimiento { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relaciones
        public ICollection<CotizacionModularDetalle> Detalles { get; set; }
    }
}
