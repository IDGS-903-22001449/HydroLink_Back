using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class CompraDetalleCreateDto
    {
        [Required]
        public int MateriaPrimaId { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        public decimal PrecioUnitario { get; set; }
    }
}
