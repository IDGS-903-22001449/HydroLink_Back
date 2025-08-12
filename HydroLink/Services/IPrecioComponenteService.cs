using HydroLink.Dtos;

namespace HydroLink.Services
{
    public interface IPrecioComponenteService
    {
        /// <summary>
        /// Obtiene el precio más reciente de un componente basado en las últimas compras
        /// </summary>
        Task<decimal> ObtenerPrecioActualAsync(int componenteId);
        
        /// <summary>
        /// Obtiene el precio promedio de un componente en los últimos N días
        /// </summary>
        Task<decimal> ObtenerPrecioPromedioAsync(int componenteId, int diasAtras = 30);
        
        /// <summary>
        /// Obtiene información completa de precios de un componente
        /// </summary>
        Task<ComponentePrecioInfoDto> ObtenerInfoPreciosAsync(int componenteId);
        
        /// <summary>
        /// Obtiene precios de múltiples componentes de manera eficiente
        /// </summary>
        Task<Dictionary<int, decimal>> ObtenerPreciosMultiplesAsync(List<int> componenteIds, TipoPrecio tipoPrecio = TipoPrecio.Actual);
    }

    public enum TipoPrecio
    {
        Actual,      // Precio de la última compra
        Promedio,    // Precio promedio de últimos 30 días
        Minimo,      // Precio mínimo de últimos 30 días
        Maximo       // Precio máximo de últimos 30 días
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
