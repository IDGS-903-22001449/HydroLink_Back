using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class CotizacionCreateDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public List<CotizacionDetalleCreateDto> Detalles { get; set; }
    }
}
