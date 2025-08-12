using HydroLink.Data;
using Microsoft.EntityFrameworkCore;

namespace HydroLink.Services
{
    public class PrecioActualizacionService : IPrecioActualizacionService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ICostoPromedioService _costoPromedioService;
        private readonly ILogger<PrecioActualizacionService> _logger;

        public PrecioActualizacionService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ICostoPromedioService costoPromedioService,
            ILogger<PrecioActualizacionService> logger)
        {
            _contextFactory = contextFactory;
            _costoPromedioService = costoPromedioService;
            _logger = logger;
        }

        public async Task ActualizarPreciosProductosPorMateriaPrimaAsync(int materiaPrimaId, decimal nuevoCosto)
        {
            // Usar contexto independiente para evitar conflictos de concurrencia
            using var context = await _contextFactory.CreateDbContextAsync();
            
            try
            {
                _logger.LogInformation("Iniciando actualización de precios para materia prima {MateriaPrimaId} con nuevo costo {NuevoCosto}", materiaPrimaId, nuevoCosto);
                
                // Obtener todos los componentes que usan esta materia prima
                var componentesAfectados = await context.ComponenteMateriaPrima
                    .Where(cmp => cmp.MateriaPrimaId == materiaPrimaId && cmp.Activo)
                    .Select(cmp => cmp.ComponenteId)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation("Encontrados {ComponenteCount} componentes afectados por materia prima {MateriaPrimaId}", componentesAfectados.Count, materiaPrimaId);

                // Actualizar precios por cada componente afectado
                foreach (var componenteId in componentesAfectados)
                {
                    await ActualizarPreciosProductosPorComponenteAsync(componenteId);
                }

                _logger.LogInformation(
                    "Precios actualizados exitosamente por cambio en materia prima {MateriaPrimaId}. Componentes afectados: {ComponenteIds}",
                    materiaPrimaId, string.Join(", ", componentesAfectados));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error al actualizar precios por cambio en materia prima {MateriaPrimaId}", 
                    materiaPrimaId);
                throw;
            }
        }

        public async Task ActualizarPreciosProductosPorComponenteAsync(int componenteId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            try
            {
                _logger.LogInformation("Iniciando actualización de precios para componente {ComponenteId}", componenteId);
                
                // Obtener todos los productos que usan este componente
                var productosAfectados = await context.ComponenteRequerido
                    .Where(cr => cr.ComponenteId == componenteId)
                    .Select(cr => cr.ProductoHydroLinkId)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation("Encontrados {ProductoCount} productos afectados por componente {ComponenteId}", productosAfectados.Count, componenteId);

                foreach (var productoId in productosAfectados)
                {
                    var nuevoPrecio = await RecalcularPrecioProductoConMargenExistenteAsync(productoId);
                    _logger.LogInformation(
                        "Precio del producto {ProductoId} actualizado a ${NuevoPrecio:F2} por cambio en componente {ComponenteId}",
                        productoId, nuevoPrecio, componenteId);
                }
                
                _logger.LogInformation("Actualización de precios completada para componente {ComponenteId}", componenteId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error al actualizar precios por cambio en componente {ComponenteId}", 
                    componenteId);
                throw;
            }
        }

        public async Task<decimal> RecalcularPrecioProductoConMargenExistenteAsync(int productoId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var producto = await context.ProductoHydroLink.FindAsync(productoId);
            if (producto == null)
            {
                throw new ArgumentException($"Producto con ID {productoId} no encontrado");
            }

            // Obtener el margen de ganancia actual
            var margenActual = await ObtenerMargenGananciaProductoAsync(productoId);

            // Calcular nuevo precio con el mismo margen
            var nuevoPrecio = await _costoPromedioService.CalcularPrecioProductoHydroLinkAsync(productoId, margenActual);

            // Guardar el precio anterior para el log
            var precioAnterior = producto.Precio;
            
            // Actualizar el precio
            producto.Precio = nuevoPrecio;
            await context.SaveChangesAsync();

            _logger.LogInformation("Precio del producto {ProductoId} actualizado: ${PrecioAnterior:F2} -> ${NuevoPrecio:F2}", 
                productoId, precioAnterior, nuevoPrecio);

            return nuevoPrecio;
        }

        public async Task<decimal> ObtenerMargenGananciaProductoAsync(int productoId)
        {
            // Para evitar el problema de recálculo circular, usar un margen fijo configurado
            // El margen se puede obtener de una configuración o usar uno estándar
            
            // Por ahora, usamos un margen fijo del 35% que se configuró previamente
            // En el futuro, esto podría venir de una tabla de configuración por producto
            var margenFijo = 0.35m; // 35% de margen
            
            _logger.LogInformation("Usando margen fijo de {Margen:P2} para producto {ProductoId}", 
                margenFijo, productoId);
            
            return margenFijo;
        }
    }
}
