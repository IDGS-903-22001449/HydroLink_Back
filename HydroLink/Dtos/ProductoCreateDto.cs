using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class ProductoCreateDto
    {
        [Required]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Required]
        public decimal PrecioVenta { get; set; }

        public List<ListaMaterialesDto> Materiales { get; set; }
    }
}
