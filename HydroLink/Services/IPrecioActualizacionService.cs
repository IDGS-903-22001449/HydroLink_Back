namespace HydroLink.Services
{
    public interface IPrecioActualizacionService
    {
        /// <summary>
        /// Actualiza automáticamente los precios de todos los productos que usan una materia prima específica
        /// </summary>
        Task ActualizarPreciosProductosPorMateriaPrimaAsync(int materiaPrimaId, decimal nuevoCosto);

        /// <summary>
        /// Actualiza automáticamente los precios de todos los productos que usan un componente específico
        /// </summary>
        Task ActualizarPreciosProductosPorComponenteAsync(int componenteId);

        /// <summary>
        /// Recalcula y actualiza el precio de un producto específico manteniendo su margen de ganancia
        /// </summary>
        Task<decimal> RecalcularPrecioProductoConMargenExistenteAsync(int productoId);

        /// <summary>
        /// Obtiene el margen de ganancia actual de un producto
        /// </summary>
        Task<decimal> ObtenerMargenGananciaProductoAsync(int productoId);
    }
}
