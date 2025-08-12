using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class ProductoHydroLink
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty;
        
        [Required]
        public decimal Precio { get; set; }
        
        public bool Activo { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        [StringLength(1000)]
        public string Especificaciones { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string TipoInstalacion { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string TiempoInstalacion { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string Garantia { get; set; } = string.Empty;
        
        // Imagen del producto en formato base64
        public string? ImagenBase64 { get; set; }
        
        // Manual de usuario del producto en formato base64
        public string? ManualUsuarioPdf { get; set; }

        // Navegación
        //public virtual ICollection<ComponenteRequerido> ComponentesRequeridos { get; set; } = new List<ComponenteRequerido>();
        public ICollection<ComponenteRequerido> ComponentesRequeridos { get; set; }
        public virtual ICollection<Cotizacion> Cotizaciones { get; set; } = new List<Cotizacion>();
    }
    
    public class ComponenteRequerido
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProductoHydroLinkId { get; set; }
        
        [Required]
        public int ComponenteId { get; set; }  // Mantener ComponenteId
        
        [Required]
        public decimal Cantidad { get; set; }
        
        [StringLength(200)]
        public string Especificaciones { get; set; } = string.Empty;
        
        // Navegación
        public virtual ProductoHydroLink ProductoHydroLink { get; set; } = null!;
        public virtual Componente Componente { get; set; }  // Mantener Componente
    }
}
