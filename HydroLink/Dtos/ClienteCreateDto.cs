using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class ClienteCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        public string Telefono { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Empresa { get; set; }
    }
}
