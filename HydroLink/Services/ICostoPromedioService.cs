using HydroLink.Dtos;

namespace HydroLink.Services
{
    public interface ICostoPromedioService
    {
        Task<decimal> CalcularCostoPromedioComponenteAsync(int componenteId);
        
        Task<decimal> CalcularCostoPromedioMateriaPrimaAsync(int materiaPrimaId);
        
        Task<Dictionary<int, decimal>> CalcularCostosMultiplesComponentesAsync(List<int> componenteIds);
        
        Task<decimal> CalcularPrecioProductoHydroLinkAsync(int productoId, decimal margenGanancia = 0.30m);
        
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
