using HydroLink.Models;
using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class CreateUsuarioDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        [Phone]
        [MaxLength(20)]
        public string Telefono { get; set; }

        [MaxLength(250)]
        public string Direccion { get; set; }

        [Required]
        [MaxLength(50)]
        public string NombreUsuario { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public RolUsuario Rol { get; set; }
    }
}
