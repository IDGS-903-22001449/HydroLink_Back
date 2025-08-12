using HydroLink.Dtos;

namespace HydroLink.Services
{
    public interface ICosteoPromedioService
    {
        /// <summary>
        /// Actualiza el costo promedio de una materia prima después de una entrada
        /// </summary>
        Task<decimal> ActualizarCostoPromedioAsync(int materiaPrimaId, int cantidadEntrada, decimal costoUnitarioEntrada, string? actualizadoPor = null);

        /// <summary>
        /// Registra una salida de materia prima usando el costo promedio actual
        /// </summary>
        Task<decimal> RegistrarSalidaAsync(int materiaPrimaId, int cantidadSalida, string? actualizadoPor = null);

        /// <summary>
        /// Obtiene el costo promedio actual de una materia prima
        /// </summary>
        Task<decimal> ObtenerCostoPromedioActualAsync(int materiaPrimaId);

        /// <summary>
        /// Obtiene información detallada del costo promedio de una materia prima
        /// </summary>
        Task<CostoPromedioInfoDto> ObtenerInfoCostoPromedioAsync(int materiaPrimaId);

        /// <summary>
        /// Inicializa el costo promedio de una materia prima por primera vez
        /// </summary>
        Task<decimal> InicializarCostoPromedioAsync(int materiaPrimaId, int cantidadInicial, decimal costoUnitarioInicial, string? actualizadoPor = null);

        /// <summary>
        /// Calcula el costo total de los componentes de un producto usando costeo por promedio
        /// </summary>
        Task<decimal> CalcularCostoTotalProductoAsync(int productoId);

        /// <summary>
        /// Calcula el precio de venta de un producto aplicando margen sobre el costo promedio
        /// </summary>
        Task<decimal> CalcularPrecioVentaProductoAsync(int productoId, decimal margenGanancia = 0.30m);

        /// <summary>
        /// Calcula el precio de un ProductoHydroLink aplicando margen sobre el costo promedio
        /// </summary>
        Task<decimal> CalcularPrecioProductoHydroLinkAsync(int productoId, decimal margenGanancia = 0.30m);
    }
}
