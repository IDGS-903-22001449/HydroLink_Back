using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    public class CostoPromedioMateriaPrima
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MateriaPrimaId { get; set; }

        [Required]
        public decimal CostoPromedioActual { get; set; }

        [Required]
        public int ExistenciaActual { get; set; }

        [Required]
        public decimal ValorInventarioTotal { get; set; }

        [Required]
        public DateTime FechaUltimaActualizacion { get; set; }

        public string? ActualizadoPor { get; set; }

        // Navegación
        public MateriaPrima MateriaPrima { get; set; }
    }
}
