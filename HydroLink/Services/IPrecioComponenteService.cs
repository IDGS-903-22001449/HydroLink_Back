using HydroLink.Dtos;

namespace HydroLink.Services
{
    public interface IPrecioComponenteService
    {
        Task<decimal> ObtenerPrecioActualAsync(int componenteId);
        Task<decimal> ObtenerPrecioPromedioAsync(int componenteId, int diasAtras = 30);
        Task<ComponentePrecioInfoDto> ObtenerInfoPreciosAsync(int componenteId);
        
        Task<Dictionary<int, decimal>> ObtenerPreciosMultiplesAsync(List<int> componenteIds, TipoPrecio tipoPrecio = TipoPrecio.Actual);
    }

    public enum TipoPrecio
    {
        Actual,
        Promedio, 
        Minimo,   
        Maximo    
    }

    public class ComponentePrecioInfoDto
    {
        public int ComponenteId { get; set; }
        public string NombreComponente { get; set; } = string.Empty;
        public decimal? PrecioActual { get; set; }
        public decimal? PrecioPromedio30Dias { get; set; }
        public decimal? PrecioMinimo30Dias { get; set; }
        public decimal? PrecioMaximo30Dias { get; set; }
        public DateTime? FechaUltimaCompra { get; set; }
        public int CantidadComprasUltimos30Dias { get; set; }
        public string ProveedorUltimaCompra { get; set; } = string.Empty;
        public bool TieneDatos { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }
}
