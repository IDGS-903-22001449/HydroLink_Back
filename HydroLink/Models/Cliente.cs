using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class Cliente : Persona
    {
        public Cliente()
        {
            TipoPersona = "Cliente";
        }
        
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        
        public bool Activo { get; set; } = true;
        
        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}

