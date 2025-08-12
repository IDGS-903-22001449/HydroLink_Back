using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class Compra
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public int ProveedorId { get; set; }
        public virtual Proveedor Proveedor { get; set; }

        public ICollection<CompraDetalle> Detalles { get; set; }
    }
}
