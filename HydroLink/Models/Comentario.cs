using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class Comentario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; }
        public AppUser Usuario { get; set; }

        [Required]
        public int ProductoHydroLinkId { get; set; }
        public ProductoHydroLink ProductoHydroLink { get; set; }

        [Required]
        [Range(1, 5)]
        public int Calificacion { get; set; }

        [MaxLength(500)]
        public string Texto { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
