using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class Componente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(500)]
        public string Descripcion { get; set; }

        [Required]
        [MaxLength(50)]
        public string Categoria { get; set; } 

        [Required]
        [MaxLength(20)]
        public string UnidadMedida { get; set; }

        public string Especificaciones { get; set; }

        public bool EsPersonalizable { get; set; } = false;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public bool Activo { get; set; } = true;

        public ICollection<CotizacionDetalle> CotizacionDetalles { get; set; }
        public ICollection<ComponenteMateriaPrima> MateriaPrimas { get; set; } = new List<ComponenteMateriaPrima>();
    }
}
