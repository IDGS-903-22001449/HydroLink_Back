namespace HydroLink.Services
{
    public interface IPrecioActualizacionService
    {
        Task ActualizarPreciosProductosPorMateriaPrimaAsync(int materiaPrimaId, decimal nuevoCosto);
        Task ActualizarPreciosProductosPorComponenteAsync(int componenteId);
        Task<decimal> RecalcularPrecioProductoConMargenExistenteAsync(int productoId);
        Task<decimal> ObtenerMargenGananciaProductoAsync(int productoId);
    }
}
