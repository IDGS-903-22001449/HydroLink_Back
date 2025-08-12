using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class ComentarioCreateDto
    {
        [Required]
        public string UsuarioId { get; set; }

        [Required]
        public int ProductoHydroLinkId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Calificacion { get; set; }

        [MaxLength(500)]
        public string Texto { get; set; }
    }
}
