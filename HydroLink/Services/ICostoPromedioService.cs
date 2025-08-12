using HydroLink.Dtos;

namespace HydroLink.Services
{
    public interface ICostoPromedioService
    {
        /// <summary>
        /// Calcula el costo promedio ponderado de un componente basado en sus materias primas
        /// </summary>
        Task<decimal> CalcularCostoPromedioComponenteAsync(int componenteId);
        
        /// <summary>
        /// Calcula el costo promedio ponderado de una materia prima usando método FIFO/Promedio
        /// </summary>
        Task<decimal> CalcularCostoPromedioMateriaPrimaAsync(int materiaPrimaId);
        
        /// <summary>
        /// Calcula múltiples costos de componentes de manera eficiente
        /// </summary>
        Task<Dictionary<int, decimal>> CalcularCostosMultiplesComponentesAsync(List<int> componenteIds);
        
        /// <summary>
        /// Calcula el precio de venta de un producto HydroLink basado en sus componentes
        /// </summary>
        Task<decimal> CalcularPrecioProductoHydroLinkAsync(int productoId, decimal margenGanancia = 0.30m);
        
        /// <summary>
        /// Obtiene información detallada del costo de un componente
        /// </summary>
        Task<ComponenteCostoDetalleDto> ObtenerDetalleCostoComponenteAsync(int componenteId);
    }
    
    public class ComponenteCostoDetalleDto
    {
        public int ComponenteId { get; set; }
        public string NombreComponente { get; set; } = string.Empty;
        public decimal CostoTotal { get; set; }
        public List<MateriaPrimaCostoDto> MateriasRimas { get; set; } = new();
        public DateTime FechaCalculo { get; set; } = DateTime.UtcNow;
        public string Observaciones { get; set; } = string.Empty;
    }
    
    public class MateriaPrimaCostoDto
    {
        public int MateriaPrimaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal CantidadNecesaria { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal CostoTotalMateria { get; set; }
        public decimal PorcentajeMerma { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
    }
}
