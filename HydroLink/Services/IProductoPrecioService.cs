using HydroLink.Dtos;

namespace HydroLink.Services
{
    public interface IProductoPrecioService
    {
        /// <summary>
        /// Calcula el precio de un producto basado en sus componentes requeridos
        /// </summary>
        Task<decimal> CalcularPrecioProductoAsync(int productoId, decimal margenGanancia = 0.30m);
        
        /// <summary>
        /// Actualiza automáticamente el precio de un producto
        /// </summary>
        Task<bool> ActualizarPrecioProductoAsync(int productoId, decimal margenGanancia = 0.30m);
        
        /// <summary>
        /// Actualiza precios de todos los productos activos
        /// </summary>
        Task<int> ActualizarTodosLosPreciosAsync(decimal margenGanancia = 0.30m);
        
        /// <summary>
        /// Obtiene información detallada del cálculo de precio de un producto
        /// </summary>
        Task<ProductoPrecioDetalleDto> ObtenerDetalleCalculoPrecioAsync(int productoId, decimal margenGanancia = 0.30m);
        
        /// <summary>
        /// Recalcula precios después de una nueva compra
        /// </summary>
        Task<int> RecalcularPreciosDespuesDeCompraAsync(List<int> componentesAfectados, decimal margenGanancia = 0.30m);
    }

    public class ProductoPrecioDetalleDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public List<ComponenteCostoDto> ComponentesCosto { get; set; } = new();
        public decimal CostoTotalComponentes { get; set; }
        public decimal MargenGanancia { get; set; }
        public decimal MontoMargen { get; set; }
        public decimal PrecioFinal { get; set; }
        public decimal PrecioAnterior { get; set; }
        public bool CambioSignificativo { get; set; } // Si el cambio es mayor al 5%
        public DateTime FechaCalculo { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }

    public class ComponenteCostoDto
    {
        public int ComponenteId { get; set; }
        public string NombreComponente { get; set; } = string.Empty;
        public decimal CantidadRequerida { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal CostoTotal { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public DateTime? FechaUltimoPrecio { get; set; }
        public bool PrecioActualizado { get; set; }
    }
}
