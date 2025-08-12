using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class CotizacionDetalleCreateDto
    {
        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int Cantidad { get; set; }
    }
}
