using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class CotizacionDetalle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CotizacionId { get; set; }
        public Cotizacion Cotizacion { get; set; }

        public int? ProductoId { get; set; }
        public Producto Producto { get; set; }

        public int ComponenteId { get; set; }
        public Componente Componente { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreItem { get; set; }

        [MaxLength(500)]
        public string Descripcion { get; set; }

        [Required]
        [MaxLength(50)]
        public string Categoria { get; set; }

        [Required]
        public decimal Cantidad { get; set; }

        [Required]
        [MaxLength(20)]
        public string UnidadMedida { get; set; }

        [Required]
        public decimal PrecioUnitarioEstimado { get; set; }

        public decimal Subtotal => Cantidad * PrecioUnitarioEstimado;

        public string Especificaciones { get; set; }
    }
}
