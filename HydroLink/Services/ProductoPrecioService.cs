using HydroLink.Data;
using HydroLink.Models;
using Microsoft.EntityFrameworkCore;

namespace HydroLink.Services
{
    public class ProductoPrecioService : IProductoPrecioService
    {
        private readonly AppDbContext _context;
        private readonly IPrecioComponenteService _precioComponenteService;

        public ProductoPrecioService(AppDbContext context, IPrecioComponenteService precioComponenteService)
        {
            _context = context;
            _precioComponenteService = precioComponenteService;
        }

        public async Task<decimal> CalcularPrecioProductoAsync(int productoId, decimal margenGanancia = 0.30m)
        {
            var producto = await _context.ProductoHydroLink
                .Include(p => p.ComponentesRequeridos)
                .FirstOrDefaultAsync(p => p.Id == productoId);

            if (producto == null)
                throw new Exception("Producto no encontrado");

            decimal costoTotalComponentes = 0m;
            foreach (var componente in producto.ComponentesRequeridos)
            {
                var precioUnitario = await _precioComponenteService.ObtenerPrecioActualAsync(componente.ComponenteId);
                costoTotalComponentes += precioUnitario * componente.Cantidad;
            }

            return costoTotalComponentes * (1 + margenGanancia);
        }

        public async Task<bool> ActualizarPrecioProductoAsync(int productoId, decimal margenGanancia = 0.30m)
        {
            var nuevoPrecio = await CalcularPrecioProductoAsync(productoId, margenGanancia);
            var producto = await _context.ProductoHydroLink.FindAsync(productoId);

            if (producto == null)
                return false;

            producto.Precio = nuevoPrecio;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> ActualizarTodosLosPreciosAsync(decimal margenGanancia = 0.30m)
        {
            var productos = await _context.ProductoHydroLink.ToListAsync();
            int count = 0;
            foreach (var producto in productos)
            {
                var updated = await ActualizarPrecioProductoAsync(producto.Id, margenGanancia);
                if (updated) count++;
            }

            return count;
        }

        public async Task<ProductoPrecioDetalleDto> ObtenerDetalleCalculoPrecioAsync(int productoId, decimal margenGanancia = 0.30m)
        {
            var producto = await _context.ProductoHydroLink
                .Include(p => p.ComponentesRequeridos)
                .ThenInclude(cr => cr.Componente)
                .FirstOrDefaultAsync(p => p.Id == productoId);

            if (producto == null)
                throw new Exception("Producto no encontrado");

            decimal costoTotalComponentes = 0m;
            List<ComponenteCostoDto> componentesCosto = new();

            foreach (var componente in producto.ComponentesRequeridos)
            {
                var precioUnitario = await _precioComponenteService.ObtenerPrecioActualAsync(componente.ComponenteId);
                var costoTotal = precioUnitario * componente.Cantidad;
                costoTotalComponentes += costoTotal;

                componentesCosto.Add(new ComponenteCostoDto
                {
                    ComponenteId = componente.ComponenteId,
                    NombreComponente = componente.Componente.Nombre,
                    CantidadRequerida = componente.Cantidad,
                    PrecioUnitario = precioUnitario,
                    CostoTotal = costoTotal,
                    UnidadMedida = componente.Componente.UnidadMedida,
                    FechaUltimoPrecio = DateTime.Now,
                    PrecioActualizado = true
                });
            }

            return new ProductoPrecioDetalleDto
            {
                ProductoId = producto.Id,
                NombreProducto = producto.Nombre,
                ComponentesCosto = componentesCosto,
                CostoTotalComponentes = costoTotalComponentes,
                MargenGanancia = margenGanancia,
                MontoMargen = costoTotalComponentes * margenGanancia,
                PrecioFinal = costoTotalComponentes * (1 + margenGanancia),
                PrecioAnterior = producto.Precio,
                CambioSignificativo = Math.Abs(producto.Precio - costoTotalComponentes * (1 + margenGanancia)) / producto.Precio > 0.05m,
                FechaCalculo = DateTime.Now,
                Observaciones = "Precio calculado automáticamente"
            };
        }

        public async Task<int> RecalcularPreciosDespuesDeCompraAsync(List<int> componentesAfectados, decimal margenGanancia = 0.30m)
        {
            var productos = await _context.ProductoHydroLink
                .Include(p => p.ComponentesRequeridos)
                .Where(p => p.ComponentesRequeridos.Any(cr => componentesAfectados.Contains(cr.ComponenteId)))
                .ToListAsync();

            int updatedCount = 0;
            foreach (var producto in productos)
            {
                var updated = await ActualizarPrecioProductoAsync(producto.Id, margenGanancia);
                if (updated) updatedCount++;
            }

            return updatedCount;
        }
    }
}
