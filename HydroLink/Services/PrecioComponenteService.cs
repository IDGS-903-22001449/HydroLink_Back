using HydroLink.Data;
using HydroLink.Dtos;
using HydroLink.Services;
using Microsoft.EntityFrameworkCore;

namespace HydroLink.Services
{
    public class PrecioComponenteService : IPrecioComponenteService
    {
        private readonly AppDbContext _context;

        public PrecioComponenteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> ObtenerPrecioActualAsync(int componenteId)
        {
            var materiaPrimasComponente = await _context.ComponenteMateriaPrima
                .Where(cm => cm.ComponenteId == componenteId && cm.Activo)
                .Include(cm => cm.MateriaPrima)
                .ToListAsync();

            if (!materiaPrimasComponente.Any())
            {
                return 0m;
            }

            decimal precioTotal = 0m;

            foreach (var materiaPrimaComponente in materiaPrimasComponente)
            {
                var precioMateriaPrima = materiaPrimaComponente.MateriaPrima.CostoUnitario;
                
                if (precioMateriaPrima == 0)
                {
                    precioMateriaPrima = await _context.CompraDetalle
                        .Where(d => d.MateriaPrimaId == materiaPrimaComponente.MateriaPrimaId)
                        .OrderByDescending(d => d.Compra.Fecha)
                        .Select(d => d.PrecioUnitario)
                        .FirstOrDefaultAsync();
                }

                var costoMateriaPrima = precioMateriaPrima * materiaPrimaComponente.CantidadConMerma;
                precioTotal += costoMateriaPrima;
            }

            return precioTotal;
        }

        public async Task<decimal> ObtenerPrecioPromedioAsync(int componenteId, int diasAtras = 30)
        {
            return await ObtenerPrecioActualAsync(componenteId);
        }

        public async Task<ComponentePrecioInfoDto> ObtenerInfoPreciosAsync(int componenteId)
        {
            var componente = await _context.Componente.FindAsync(componenteId);
            if (componente == null)
                return new ComponentePrecioInfoDto { Observaciones = "Componente no encontrado" };

            var materiaPrimasComponente = await _context.ComponenteMateriaPrima
                .Where(cm => cm.ComponenteId == componenteId && cm.Activo)
                .Include(cm => cm.MateriaPrima)
                .ToListAsync();

            var info = new ComponentePrecioInfoDto
            {
                ComponenteId = componenteId,
                NombreComponente = componente.Nombre,
                TieneDatos = materiaPrimasComponente.Any()
            };

            if (info.TieneDatos)
            {
                info.PrecioActual = await ObtenerPrecioActualAsync(componenteId);
                info.PrecioPromedio30Dias = info.PrecioActual; 
                info.PrecioMinimo30Dias = info.PrecioActual;
                info.PrecioMaximo30Dias = info.PrecioActual;
                
                var materiasPrimasIds = materiaPrimasComponente.Select(mp => mp.MateriaPrimaId).ToList();
                var ultimaCompra = await _context.CompraDetalle
                    .Where(d => materiasPrimasIds.Contains(d.MateriaPrimaId))
                    .Include(d => d.Compra.Proveedor)
                    .OrderByDescending(d => d.Compra.Fecha)
                    .FirstOrDefaultAsync();
                    
                if (ultimaCompra != null)
                {
                    info.FechaUltimaCompra = ultimaCompra.Compra.Fecha;
                    info.ProveedorUltimaCompra = ultimaCompra.Compra.Proveedor?.Nombre ?? "N/A";
                }
                
                info.CantidadComprasUltimos30Dias = materiaPrimasComponente.Count;
                info.Observaciones = $"Precio calculado basado en {materiaPrimasComponente.Count} materia(s) prima(s)";
            } 
            else 
            {
                info.Observaciones = "No hay materias primas asociadas a este componente";
            }
            
            return info;
        }

        public async Task<Dictionary<int, decimal>> ObtenerPreciosMultiplesAsync(List<int> componenteIds, TipoPrecio tipoPrecio = TipoPrecio.Actual)
        {
            var precios = new Dictionary<int, decimal>();

            foreach (var id in componenteIds)
            {
                precios[id] = tipoPrecio switch
                {
                    TipoPrecio.Promedio => await ObtenerPrecioPromedioAsync(id),
                    _ => await ObtenerPrecioActualAsync(id)
                };
            }

            return precios;
        }
    }
}
