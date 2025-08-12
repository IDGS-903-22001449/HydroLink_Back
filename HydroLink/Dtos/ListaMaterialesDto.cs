using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class ListaMaterialesDto
    {
        [Required]
        public int MateriaPrimaId { get; set; }

        [Required]
        public decimal Cantidad { get; set; }
    }
}
