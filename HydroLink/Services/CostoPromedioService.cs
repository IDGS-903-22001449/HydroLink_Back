using HydroLink.Data;
using HydroLink.Services;
using Microsoft.EntityFrameworkCore;

namespace HydroLink.Services
{
    public class CostoPromedioService : ICostoPromedioService
    {
        private readonly AppDbContext _context;
        
        public CostoPromedioService(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<decimal> CalcularCostoPromedioComponenteAsync(int componenteId)
        {
            // Obtener todas las materias primas del componente
            var componenteMaterias = await _context.ComponenteMateriaPrima
                .Where(cm => cm.ComponenteId == componenteId && cm.Activo)
                .Include(cm => cm.MateriaPrima)
                .ToListAsync();
                
            if (!componenteMaterias.Any())
            {
                // Si no tiene materias primas asociadas, intentar obtener de CompraDetalle directamente
                return await ObtenerCostoDirectoComponenteAsync(componenteId);
            }
            
            decimal costoTotal = 0;
            
            foreach (var componenteMateria in componenteMaterias)
            {
                var costoMateriaPrima = await CalcularCostoPromedioMateriaPrimaAsync(componenteMateria.MateriaPrimaId);
                var cantidadConMerma = componenteMateria.CantidadConMerma;
                
                costoTotal += costoMateriaPrima * cantidadConMerma;
            }
            
            return costoTotal;
        }
        
        public async Task<decimal> CalcularCostoPromedioMateriaPrimaAsync(int materiaPrimaId)
        {
            // Obtener el costo promedio directamente de la tabla MateriaPrima
            // que se actualiza automáticamente en ComprasController
            var materiaPrima = await _context.MateriaPrima.FindAsync(materiaPrimaId);
            
            if (materiaPrima == null || materiaPrima.CostoUnitario == 0)
            {
                // Fallback: obtener el costo de las últimas compras
                var ultimaCompra = await _context.CompraDetalle
                    .Where(cd => cd.MateriaPrimaId == materiaPrimaId)
                    .OrderByDescending(cd => cd.Compra.Fecha)
                    .FirstOrDefaultAsync();
                    
                return ultimaCompra?.PrecioUnitario ?? 0;
            }
            
            return materiaPrima.CostoUnitario;
        }
        
        public async Task<Dictionary<int, decimal>> CalcularCostosMultiplesComponentesAsync(List<int> componenteIds)
        {
            var costos = new Dictionary<int, decimal>();
            
            foreach (var componenteId in componenteIds)
            {
                costos[componenteId] = await CalcularCostoPromedioComponenteAsync(componenteId);
            }
            
            return costos;
        }
        
        public async Task<decimal> CalcularPrecioProductoHydroLinkAsync(int productoId, decimal margenGanancia = 0.30m)
        {
            var producto = await _context.ProductoHydroLink
                .Include(p => p.ComponentesRequeridos)
                .FirstOrDefaultAsync(p => p.Id == productoId);
                
            if (producto == null)
                return 0;
                
            decimal costoTotal = 0;
            
            foreach (var componenteRequerido in producto.ComponentesRequeridos)
            {
                var costoComponente = await CalcularCostoPromedioComponenteAsync(componenteRequerido.ComponenteId);
                costoTotal += costoComponente * componenteRequerido.Cantidad;
            }
            
            // Aplicar margen de ganancia
            return costoTotal * (1 + margenGanancia);
        }
        
        public async Task<ComponenteCostoDetalleDto> ObtenerDetalleCostoComponenteAsync(int componenteId)
        {
            var componente = await _context.Componente.FindAsync(componenteId);
            if (componente == null)
            {
                return new ComponenteCostoDetalleDto
                {
                    ComponenteId = componenteId,
                    Observaciones = "Componente no encontrado"
                };
            }
            
            var componenteMaterias = await _context.ComponenteMateriaPrima
                .Where(cm => cm.ComponenteId == componenteId && cm.Activo)
                .Include(cm => cm.MateriaPrima)
                .ToListAsync();
                
            var detalle = new ComponenteCostoDetalleDto
            {
                ComponenteId = componenteId,
                NombreComponente = componente.Nombre
            };
            
            decimal costoTotal = 0;
            
            foreach (var componenteMateria in componenteMaterias)
            {
                var costoUnitario = await CalcularCostoPromedioMateriaPrimaAsync(componenteMateria.MateriaPrimaId);
                var cantidadConMerma = componenteMateria.CantidadConMerma;
                var costoTotalMateria = costoUnitario * cantidadConMerma;
                
                costoTotal += costoTotalMateria;
                
                detalle.MateriasRimas.Add(new MateriaPrimaCostoDto
                {
                    MateriaPrimaId = componenteMateria.MateriaPrimaId,
                    Nombre = componenteMateria.MateriaPrima.Name,
                    CantidadNecesaria = cantidadConMerma,
                    CostoUnitario = costoUnitario,
                    CostoTotalMateria = costoTotalMateria,
                    PorcentajeMerma = componenteMateria.PorcentajeMerma,
                    UnidadMedida = componenteMateria.MateriaPrima.UnidadMedida
                });
            }
            
            detalle.CostoTotal = costoTotal;
            
            return detalle;
        }
        
        private async Task<decimal> ObtenerCostoDirectoComponenteAsync(int componenteId)
        {
            // Para componentes que no tienen materias primas asociadas
            // (como mano de obra), buscar en CompraDetalle
            var ultimaCompra = await _context.CompraDetalle
                .Where(cd => cd.MateriaPrimaId == componenteId)
                .OrderByDescending(cd => cd.Compra.Fecha)
                .FirstOrDefaultAsync();
                
            return ultimaCompra?.PrecioUnitario ?? 0;
        }
    }
}
