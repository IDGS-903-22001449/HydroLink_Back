using HydroLink.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HydroLink.Dtos
{
    public class ProductoHydroLinkDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal PrecioEstimadoComponentes { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Especificaciones { get; set; } = string.Empty;
        public string TipoInstalacion { get; set; } = string.Empty;
        public string TiempoInstalacion { get; set; } = string.Empty;
        public string Garantia { get; set; } = string.Empty;
        public decimal CostoEstimado { get; set; }
        public decimal MargenGanancia { get; set; }
        public string? ImagenBase64 { get; set; }
        
        // Para optimización de rendimiento: indicar si tiene manual sin cargar el contenido
        public bool TieneManual { get; set; }
        
        // Solo se llena cuando específicamente se solicita el PDF completo
        public string? ManualUsuarioPdf { get; set; }
        
        public List<ComponenteRequeridoDto> ComponentesRequeridos { get; set; } = new List<ComponenteRequeridoDto>();
    }

    public class ProductoHydroLinkCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty;
        
        // Precio es opcional si CalcularPrecioAutomatico es true
        public decimal? Precio { get; set; }
        
        [StringLength(1000)]
        public string Especificaciones { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string TipoInstalacion { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string TiempoInstalacion { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string Garantia { get; set; } = string.Empty;
        
        public string? ImagenBase64 { get; set; }
        
        public string? ManualUsuarioPdf { get; set; }
        
        [Required]
        public List<ComponenteRequeridoCreateDto> ComponentesRequeridos { get; set; } = new List<ComponenteRequeridoCreateDto>();
        
        // Por defecto calculamos automáticamente
        public bool CalcularPrecioAutomatico { get; set; } = true;
        
        [Range(0.01, 1.0, ErrorMessage = "El margen de ganancia debe estar entre 1% y 100%")]
        public decimal MargenGanancia { get; set; } = 0.30m; // 30% por defecto
    }

    public class ComponenteRequeridoDto
    {
        public int Id { get; set; }
        public int ComponenteId { get; set; }
        [ForeignKey("ComponenteId")]
        public Componente? Componente { get; set; } 
        public string NombreComponente { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public string Especificaciones { get; set; } = string.Empty;
    }

    public class ComponenteRequeridoCreateDto
    {
        public int ComponenteId { get; set; }
        public decimal Cantidad { get; set; }
        public string Especificaciones { get; set; } = string.Empty;
    }

    // DTO simplificado para mostrar en el home
    public class ProductoHomeDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string? ImagenBase64 { get; set; }
    }
}
