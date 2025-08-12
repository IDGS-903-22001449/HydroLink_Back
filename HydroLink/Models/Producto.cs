using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; }

        [MaxLength(500)]
        public string Descripcion { get; set; }

        [Required]
        public decimal PrecioVenta { get; set; }

        public ICollection<ListaMaterial> Materiales { get; set; }
        public ICollection<VentaDetalle> Ventas { get; set; }
        public ICollection<Comentario> Comentarios { get; set; }
    }
}
