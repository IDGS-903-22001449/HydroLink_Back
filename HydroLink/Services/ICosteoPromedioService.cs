using HydroLink.Dtos;

namespace HydroLink.Services
{
    public interface ICosteoPromedioService
    {
        Task<decimal> ActualizarCostoPromedioAsync(int materiaPrimaId, int cantidadEntrada, decimal costoUnitarioEntrada, string? actualizadoPor = null);

        Task<decimal> RegistrarSalidaAsync(int materiaPrimaId, int cantidadSalida, string? actualizadoPor = null);

        Task<decimal> ObtenerCostoPromedioActualAsync(int materiaPrimaId);

        Task<CostoPromedioInfoDto> ObtenerInfoCostoPromedioAsync(int materiaPrimaId);

        Task<decimal> InicializarCostoPromedioAsync(int materiaPrimaId, int cantidadInicial, decimal costoUnitarioInicial, string? actualizadoPor = null);

        Task<decimal> CalcularCostoTotalProductoAsync(int productoId);

        Task<decimal> CalcularPrecioVentaProductoAsync(int productoId, decimal margenGanancia = 0.30m);

        Task<decimal> CalcularPrecioProductoHydroLinkAsync(int productoId, decimal margenGanancia = 0.30m);
    }
}
