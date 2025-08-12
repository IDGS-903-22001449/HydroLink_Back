using HydroLink.Data;
using HydroLink.Dtos;
using HydroLink.Models;
using Microsoft.EntityFrameworkCore;

namespace HydroLink.Services
{
    public class CosteoPromedioService : ICosteoPromedioService
    {
        private readonly AppDbContext _context;

        public CosteoPromedioService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> ActualizarCostoPromedioAsync(int materiaPrimaId, int cantidadEntrada, decimal costoUnitarioEntrada, string? actualizadoPor = null)
        {
            var costoPromedio = await _context.CostoPromedioMateriaPrima.FirstOrDefaultAsync(cp => cp.MateriaPrimaId == materiaPrimaId);
            if (costoPromedio == null)
                return 0;

            var nuevoValorInventario = costoPromedio.ValorInventarioTotal + cantidadEntrada * costoUnitarioEntrada;
            var nuevaExistencia = costoPromedio.ExistenciaActual + cantidadEntrada;

            var nuevoCostoPromedio = nuevaExistencia > 0 ? nuevoValorInventario / nuevaExistencia : 0;

            costoPromedio.ExistenciaActual = nuevaExistencia;
            costoPromedio.CostoPromedioActual = nuevoCostoPromedio;
            costoPromedio.ValorInventarioTotal = nuevoValorInventario;
            costoPromedio.FechaUltimaActualizacion = DateTime.UtcNow;
            costoPromedio.ActualizadoPor = actualizadoPor;

            await _context.SaveChangesAsync();

            return nuevoCostoPromedio;
        }

        public async Task<decimal> RegistrarSalidaAsync(int materiaPrimaId, int cantidadSalida, string? actualizadoPor = null)
        {
            var costoPromedio = await _context.CostoPromedioMateriaPrima.FirstOrDefaultAsync(cp => cp.MateriaPrimaId == materiaPrimaId);
            if (costoPromedio == null || costoPromedio.ExistenciaActual < cantidadSalida)
                return 0;

            costoPromedio.ExistenciaActual -= cantidadSalida;
            costoPromedio.ValorInventarioTotal -= cantidadSalida * costoPromedio.CostoPromedioActual;
            costoPromedio.FechaUltimaActualizacion = DateTime.UtcNow;
            costoPromedio.ActualizadoPor = actualizadoPor;

            await _context.SaveChangesAsync();

            return costoPromedio.CostoPromedioActual;
        }

        public async Task<decimal> ObtenerCostoPromedioActualAsync(int materiaPrimaId)
        {
            var costoPromedio = await _context.CostoPromedioMateriaPrima.FirstOrDefaultAsync(cp => cp.MateriaPrimaId == materiaPrimaId);
            return costoPromedio?.CostoPromedioActual ?? 0;
        }

        public async Task<CostoPromedioInfoDto> ObtenerInfoCostoPromedioAsync(int materiaPrimaId)
        {
            var costoPromedio = await _context.CostoPromedioMateriaPrima.Include(cp => cp.MateriaPrima).FirstOrDefaultAsync(cp => cp.MateriaPrimaId == materiaPrimaId);

            if (costoPromedio == null)
            {
                return new CostoPromedioInfoDto
                {
                    MateriaPrimaId = materiaPrimaId,
                    Observaciones = "No existe información de costo promedio"
                };
            }

            return new CostoPromedioInfoDto
            {
                MateriaPrimaId = materiaPrimaId,
                NombreMateriaPrima = costoPromedio.MateriaPrima.Name,
                UnidadMedida = costoPromedio.MateriaPrima.UnidadMedida,
                CostoPromedioActual = costoPromedio.CostoPromedioActual,
                ExistenciaActual = costoPromedio.ExistenciaActual,
                ValorInventarioTotal = costoPromedio.ValorInventarioTotal,
                FechaUltimaActualizacion = costoPromedio.FechaUltimaActualizacion,
                ActualizadoPor = costoPromedio.ActualizadoPor,
                TieneDatos = true
            };
        }

        public async Task<decimal> InicializarCostoPromedioAsync(int materiaPrimaId, int cantidadInicial, decimal costoUnitarioInicial, string? actualizadoPor = null)
        {
            var costoPromedio = new CostoPromedioMateriaPrima
            {
                MateriaPrimaId = materiaPrimaId,
                CostoPromedioActual = costoUnitarioInicial,
                ExistenciaActual = cantidadInicial,
                ValorInventarioTotal = cantidadInicial * costoUnitarioInicial,
                FechaUltimaActualizacion = DateTime.UtcNow,
                ActualizadoPor = actualizadoPor
            };

            await _context.CostoPromedioMateriaPrima.AddAsync(costoPromedio);
            await _context.SaveChangesAsync();

            return costoPromedio.CostoPromedioActual;
        }

        public async Task<decimal> CalcularCostoTotalProductoAsync(int productoId)
        {
            var producto = await _context.ProductoHydroLink
                .Include(p => p.ComponentesRequeridos)
                    .ThenInclude(cr => cr.Componente)
                        .ThenInclude(c => c.MateriaPrimas)
                            .ThenInclude(cmp => cmp.MateriaPrima)
                .FirstOrDefaultAsync(p => p.Id == productoId);
            
            if (producto == null)
                return 0;

            decimal costoTotal = 0;
            foreach (var componenteRequerido in producto.ComponentesRequeridos)
            {
                // Calcular el costo del componente basado en sus materias primas
                decimal costoComponente = 0;
                
                if (componenteRequerido.Componente?.MateriaPrimas != null)
                {
                    foreach (var materiaPrimaComponente in componenteRequerido.Componente.MateriaPrimas)
                    {
                        var costoMateriaPrima = await ObtenerCostoPromedioActualAsync(materiaPrimaComponente.MateriaPrimaId);
                        var cantidadNecesaria = materiaPrimaComponente.CantidadNecesaria * materiaPrimaComponente.FactorConversion;
                        var cantidadConMerma = cantidadNecesaria * (1 + materiaPrimaComponente.PorcentajeMerma);
                        
                        costoComponente += costoMateriaPrima * cantidadConMerma;
                    }
                }
                
                costoTotal += costoComponente * componenteRequerido.Cantidad;
            }

            return costoTotal;
        }

        public async Task<decimal> CalcularPrecioVentaProductoAsync(int productoId, decimal margenGanancia = 0.30m)
        {
            var costoTotal = await CalcularCostoTotalProductoAsync(productoId);
            return costoTotal * (1 + margenGanancia);
        }

        public async Task<decimal> CalcularPrecioProductoHydroLinkAsync(int productoId, decimal margenGanancia = 0.30m)
        {
            return await CalcularPrecioVentaProductoAsync(productoId, margenGanancia);
        }
    }
}
