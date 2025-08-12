using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class ListaMaterial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }

        [Required]
        public int MateriaPrimaId { get; set; }
        public MateriaPrima MateriaPrima { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Cantidad { get; set; }
    }
}
