using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class ComponenteDto
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Categoria { get; set; } = string.Empty;
        
        public decimal PrecioUnitario { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string UnidadMedida { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Especificaciones { get; set; } = string.Empty;
        
        public bool EsPersonalizable { get; set; }
        
        public bool Activo { get; set; }
        
        public DateTime FechaCreacion { get; set; }
    }
}
