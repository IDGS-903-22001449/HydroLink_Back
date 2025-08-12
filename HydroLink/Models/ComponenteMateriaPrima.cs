using System.ComponentModel.DataAnnotations;

namespace HydroLink.Models
{
    /// <summary>
    /// Relaciona un componente (usado en cotizaciones) con materias primas (compradas a proveedores)
    /// Un componente puede estar formado por múltiples materias primas
    /// </summary>
    public class ComponenteMateriaPrima
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ComponenteId { get; set; }
        public Componente Componente { get; set; }

        [Required]
        public int MateriaPrimaId { get; set; }
        public MateriaPrima MateriaPrima { get; set; }

        /// <summary>
        /// Cantidad de esta materia prima necesaria para producir 1 unidad del componente
        /// </summary>
        [Required]
        public decimal CantidadNecesaria { get; set; } = 1.0m;

        /// <summary>
        /// Factor de conversión si las unidades difieren (ej: componente en "unidad", materia prima en "kg")
        /// </summary>
        public decimal FactorConversion { get; set; } = 1.0m;

        /// <summary>
        /// Porcentaje de desperdicio o merma esperado (0.05 = 5%)
        /// </summary>
        public decimal PorcentajeMerma { get; set; } = 0.0m;

        /// <summary>
        /// Si es el material principal o un componente secundario
        /// </summary>
        public bool EsPrincipal { get; set; } = true;

        /// <summary>
        /// Notas sobre el uso de esta materia prima en el componente
        /// </summary>
        public string Notas { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public bool Activo { get; set; } = true;

        /// <summary>
        /// Calcula la cantidad real necesaria incluyendo merma
        /// </summary>
        public decimal CantidadConMerma => CantidadNecesaria * FactorConversion * (1 + PorcentajeMerma);
    }
}
