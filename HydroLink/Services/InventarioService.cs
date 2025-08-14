
using HydroLink.Data;
using HydroLink.Models;
using Microsoft.EntityFrameworkCore;

namespace HydroLink.Services
{
    public class InventarioService : IInventarioService
    {
        private readonly AppDbContext _context;
        private readonly ICostoPromedioService _costoPromedioService;

        public InventarioService(AppDbContext context, ICostoPromedioService costoPromedioService)
        {
            _context = context;
            _costoPromedioService = costoPromedioService;
        }

        public async Task<bool> ReducirInventarioAsync(int componenteId, decimal cantidad)
        {
            var existenciaActual = await ObtenerExistenciaAsync(componenteId);
            if (existenciaActual < cantidad)
            {
                throw new InvalidOperationException($"No hay suficiente inventario para el componente ID {componenteId}. Existencia actual: {existenciaActual}, Cantidad requerida: {cantidad}");
            }

            await ReducirMaterisPrimasComponenteAsync(componenteId, cantidad);

            var costoUnitarioComponente = await _costoPromedioService.CalcularCostoPromedioComponenteAsync(componenteId);

            var movimiento = new MovimientoComponente
            {
                ComponenteId = componenteId,
                TipoMovimiento = "SALIDA",
                Cantidad = cantidad,
                FechaMovimiento = DateTime.UtcNow,
                PrecioUnitario = costoUnitarioComponente,
                CostoTotal = costoUnitarioComponente * cantidad,
                NumeroLote = $"VENTA-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Observaciones = "Salida por venta de producto"
            };

            _context.MovimientoComponente.Add(movimiento);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AumentarInventarioAsync(int componenteId, decimal cantidad)
        {
            var movimiento = new MovimientoComponente
            {
                ComponenteId = componenteId,
                TipoMovimiento = "ENTRADA",
                Cantidad = cantidad,
                FechaMovimiento = DateTime.UtcNow,
                PrecioUnitario = 0,
                CostoTotal = 0,
                NumeroLote = $"AJUSTE-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Observaciones = "Entrada por ajuste de inventario"
            };

            _context.MovimientoComponente.Add(movimiento);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> ObtenerExistenciaAsync(int componenteId)
        {
            var relacionesMateriaPrima = await _context.ComponenteMateriaPrima
                .Where(cm => cm.ComponenteId == componenteId && cm.Activo)
                .Include(cm => cm.MateriaPrima)
                .ToListAsync();

            if (!relacionesMateriaPrima.Any())
            {
                return 0; 
            }

            decimal menorDisponible = decimal.MaxValue;
            
            foreach (var relacion in relacionesMateriaPrima)
            {
                var stockMateriaPrima = relacion.MateriaPrima?.Stock ?? 0;
                var componentesPosibles = Math.Floor(stockMateriaPrima / relacion.CantidadConMerma);
                menorDisponible = Math.Min(menorDisponible, componentesPosibles);
            }

            return menorDisponible == decimal.MaxValue ? 0 : Math.Max(0, menorDisponible);
        }

        public async Task<bool> ValidarExistenciaSuficienteAsync(int componenteId, decimal cantidadRequerida)
        {
            var existenciaActual = await ObtenerExistenciaAsync(componenteId);
            return existenciaActual >= cantidadRequerida;
        }
        
        private async Task ReducirMaterisPrimasComponenteAsync(int componenteId, decimal cantidadComponente)
        {
            var materiasPrimas = await _context.ComponenteMateriaPrima
                .Where(cm => cm.ComponenteId == componenteId && cm.Activo)
                .ToListAsync();
                
            foreach (var materia in materiasPrimas)
            {
                var cantidadNecesaria = materia.CantidadConMerma * cantidadComponente;
                await ReducirInventarioMateriaPrimaAsync(materia.MateriaPrimaId, cantidadNecesaria);
            }
        }

        private async Task ReducirInventarioMateriaPrimaAsync(int materiaPrimaId, decimal cantidad)
        {
            var materiaPrima = await _context.MateriaPrima.FindAsync(materiaPrimaId);
            
            if (materiaPrima == null)
            {
                throw new InvalidOperationException($"No se encontró la materia prima con ID {materiaPrimaId}");
            }

            if (materiaPrima.Stock < cantidad)
            {
                throw new InvalidOperationException($"No hay suficiente stock para la materia prima ID {materiaPrimaId}. Stock actual: {materiaPrima.Stock}, Cantidad requerida: {cantidad}");
            }

            int cantidadInt = (int)Math.Floor(cantidad);
            materiaPrima.Stock -= cantidadInt;

            var movimiento = new MovimientoInventario
            {
                MateriaPrimaId = materiaPrimaId,
                FechaMovimiento = DateTime.UtcNow,
                TipoMovimiento = "SALIDA",
                Cantidad = cantidadInt,
                PrecioUnitario = materiaPrima.CostoUnitario,
                CostoTotal = materiaPrima.CostoUnitario * cantidad,
                NumeroLote = $"VENTA-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Observaciones = "Salida para producción de componente en venta"
            };

            _context.MovimientoInventario.Add(movimiento);
        }
    }
}
