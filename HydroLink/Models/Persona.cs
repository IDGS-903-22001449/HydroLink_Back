using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public abstract class Persona
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(15)]
        public string Telefono { get; set; }

        public string Direccion { get; set; }

        [Required]
        public string TipoPersona { get; set; }
        
        [StringLength(100)]
        public string? Empresa { get; set; }
        public virtual ICollection<Cotizacion> Cotizaciones { get; set; } = new List<Cotizacion>();
    }
}
