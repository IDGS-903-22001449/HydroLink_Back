using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class CompraCreateDto
    {
        [Required]
        public int ProveedorId { get; set; }

        [Required]
        public List<CompraDetalleCreateDto> Detalles { get; set; }
    }
}
