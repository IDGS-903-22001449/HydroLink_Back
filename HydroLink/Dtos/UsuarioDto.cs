using HydroLink.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HydroLink.Dtos
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string NombreCompleto {  get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string NombreUsuario { get; set; }
        public RolUsuario Rol { get; set; }
    }
}
