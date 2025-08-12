using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class Usuario : Persona
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public RolUsuario Rol { get; set; }
        public virtual ICollection<Venta> Ventas { get; set; }

        public virtual ICollection<Comentario> Comentarios { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }

    public enum RolUsuario 
    {
        Cliente,
        Admin,
    }
}
