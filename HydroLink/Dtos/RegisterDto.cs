using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace HydroLink.Dtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Description("Dirección de correo electrónico del usuario")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Description("Nombre completo del usuario")]
        public string FullName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        [DataType(DataType.Password)]
        [Description("Contraseña del usuario (mínimo 6 caracteres)")]
        public string Password { get; set; } = string.Empty;
        
        [Description("Roles del usuario (opcional, por defecto será 'User')")]
        public List<string> Roles { get; set; } = new List<string>();
        
        [StringLength(15, ErrorMessage = "El teléfono no puede exceder 15 caracteres")]
        [Description("Número de teléfono del cliente (opcional)")]
        public string? Telefono { get; set; }
        
        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        [Description("Dirección del cliente (opcional)")]
        public string? Direccion { get; set; }
        
        [StringLength(100, ErrorMessage = "El nombre de la empresa no puede exceder 100 caracteres")]
        [Description("Nombre de la empresa del cliente (opcional)")]
        public string? Empresa { get; set; }
    }
}
