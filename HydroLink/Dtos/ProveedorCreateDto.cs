using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class ProveedorCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Telefono { get; set; }

        public string Direccion { get; set; }

        [Required]
        [MaxLength(100)]
        public string Empresa { get; set; }
    }
}
