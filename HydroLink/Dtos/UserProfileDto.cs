namespace HydroLink.Dtos
{
    public class UserProfileDto
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string[]? Roles { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public int AccessFailedCount { get; set; }
        
        public int? ClienteId { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Empresa { get; set; }
        public string? TipoPersona { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public bool? Activo { get; set; }
    }
}
