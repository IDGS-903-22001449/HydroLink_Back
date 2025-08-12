using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class Proveedor : Persona
    {
        public virtual ICollection<Compra> Compras { get; set; }
    }
}
