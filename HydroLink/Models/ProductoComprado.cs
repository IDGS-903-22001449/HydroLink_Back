using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class ProductoComprado
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int ProductoId { get; set; }

        [Required]
        public DateTime FechaCompra { get; set; } = DateTime.UtcNow;

        public int? VentaId { get; set; }

        // Navegación
        public virtual AppUser Usuario { get; set; } = null!;
        public virtual ProductoHydroLink Producto { get; set; } = null!;
        public virtual Venta? Venta { get; set; }
    }
}
