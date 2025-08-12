using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class VentaCreateDto
    {
        [Required]
        public int ClienteId { get; set; }
        
        [Required]
        public int ProductoId { get; set; }
        
        [Required]
        public int Cantidad { get; set; }
        
        public string Observaciones { get; set; } = string.Empty;
    }
}
