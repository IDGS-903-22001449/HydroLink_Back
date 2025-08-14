using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        [MaxLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres")]
        public string Apellido { get; set; }

        [MaxLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres")]
        public string? Telefono { get; set; }

        [MaxLength(255, ErrorMessage = "La dirección no puede exceder los 255 caracteres")]
        public string? Direccion { get; set; }

        [MaxLength(100, ErrorMessage = "La empresa no puede exceder los 100 caracteres")]
        public string? Empresa { get; set; }

        [Required(ErrorMessage = "El nombre completo es requerido")]
        [MaxLength(200, ErrorMessage = "El nombre completo no puede exceder los 200 caracteres")]
        public string FullName { get; set; }

        [MaxLength(15, ErrorMessage = "El número de teléfono no puede exceder los 15 caracteres")]
        public string? PhoneNumber { get; set; }
    }
}
