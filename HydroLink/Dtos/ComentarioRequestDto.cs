using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class ComentarioRequestDto
    {
        [Required]
        public ComentarioCreateDto ComentarioDto { get; set; }
    }

    public class ComentarioCreateDtoWithGuid
    {
        [Required]
        public string UsuarioId { get; set; } 

        [Required]
        public int ProductoId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Calificacion { get; set; }

        [MaxLength(500)]
        public string Texto { get; set; }
    }
}
