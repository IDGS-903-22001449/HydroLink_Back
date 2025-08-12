using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class ComponenteCreateDto
    {
        [Required(ErrorMessage = "El nombre del componente es requerido")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La categoría es requerida")]
        [MaxLength(50)]
        public string Categoria { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La unidad de medida es requerida")]
        [MaxLength(20)]
        public string UnidadMedida { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Especificaciones { get; set; } = string.Empty;
        
        public bool EsPersonalizable { get; set; } = false;
    }
}
