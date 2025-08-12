using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class CompraDetalle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompraId { get; set; }
        public Compra Compra { get; set; }

        [Required]
        public int MateriaPrimaId { get; set; }
        public MateriaPrima MateriaPrima { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PrecioUnitario { get; set; }
    }
}
