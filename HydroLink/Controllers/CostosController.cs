using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HydroLink.Services;
using Microsoft.EntityFrameworkCore;
using HydroLink.Data;

namespace HydroLink.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CostosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICostoPromedioService _costoPromedioService;
        private readonly IPrecioActualizacionService _precioActualizacionService;
        private readonly ILogger<CostosController> _logger;

        public CostosController(
            AppDbContext context,
            ICostoPromedioService costoPromedioService,
            IPrecioActualizacionService precioActualizacionService,
            ILogger<CostosController> logger)
        {
            _context = context;
            _costoPromedioService = costoPromedioService;
            _precioActualizacionService = precioActualizacionService;
            _logger = logger;
        }

        [HttpGet("componente/{componenteId}")]
        public async Task<ActionResult<ComponenteCostoDetalleDto>> GetDetalleComponente(int componenteId)
        {
            try
            {
                var detalle = await _costoPromedioService.ObtenerDetalleCostoComponenteAsync(componenteId);
                return Ok(detalle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle de costo del componente {ComponenteId}", componenteId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("producto/{productoId}/precio")]
        public async Task<ActionResult<object>> CalcularPrecioProducto(int productoId, [FromQuery] decimal margen = 0.30m)
        {
            try
            {
                var producto = await _context.ProductoHydroLink
                    .Include(p => p.ComponentesRequeridos)
                        .ThenInclude(cr => cr.Componente)
                    .FirstOrDefaultAsync(p => p.Id == productoId);

                if (producto == null)
                {
                    return NotFound(new { message = $"Producto con ID {productoId} no encontrado" });
                }

                var precioCalculado = await _costoPromedioService.CalcularPrecioProductoHydroLinkAsync(productoId, margen);
                var detalleComponentes = new List<object>();
                decimal costoTotalComponentes = 0;

                foreach (var componenteRequerido in producto.ComponentesRequeridos)
                {
                    var costoComponente = await _costoPromedioService.CalcularCostoPromedioComponenteAsync(componenteRequerido.ComponenteId);
                    var costoTotalComponente = costoComponente * componenteRequerido.Cantidad;
                    costoTotalComponentes += costoTotalComponente;

                    detalleComponentes.Add(new
                    {
                        ComponenteId = componenteRequerido.ComponenteId,
                        Nombre = componenteRequerido.Componente?.Nombre,
                        CantidadRequerida = componenteRequerido.Cantidad,
                        CostoUnitario = costoComponente,
                        CostoTotal = costoTotalComponente
                    });
                }

                var margenCalculado = precioCalculado - costoTotalComponentes;

                var resultado = new
                {
                    ProductoId = productoId,
                    NombreProducto = producto.Nombre,
                    PrecioActual = producto.Precio,
                    PrecioCalculado = precioCalculado,
                    CostoTotalComponentes = costoTotalComponentes,
                    MargenAplicado = margen,
                    MargenEnPesos = margenCalculado,
                    PorcentajeMargenReal = costoTotalComponentes > 0 ? (margenCalculado / costoTotalComponentes) * 100 : 0,
                    FechaCalculo = DateTime.UtcNow,
                    DetalleComponentes = detalleComponentes
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular precio del producto {ProductoId}", productoId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("materiaprima/{materiaPrimaId}/lotes")]
        public async Task<ActionResult<object>> GetLotesMateriaPrima(int materiaPrimaId)
        {
            try
            {
                var materiaPrima = await _context.MateriaPrima.FindAsync(materiaPrimaId);
                if (materiaPrima == null)
                {
                    return NotFound(new { message = $"Materia prima con ID {materiaPrimaId} no encontrada" });
                }

                var lotes = await _context.LoteInventario
                    .Where(l => l.MateriaPrimaId == materiaPrimaId && l.CantidadDisponible > 0)
                    .OrderBy(l => l.FechaIngreso)
                    .Select(l => new
                    {
                        l.Id,
                        l.NumeroLote,
                        l.FechaIngreso,
                        l.CantidadInicial,
                        l.CantidadDisponible,
                        l.CostoUnitario,
                        l.CostoTotal,
                        Proveedor = l.Proveedor != null ? l.Proveedor.Nombre + " " + l.Proveedor.Apellido : "N/A"
                    })
                    .ToListAsync();

                var costoPromedio = await _costoPromedioService.CalcularCostoPromedioMateriaPrimaAsync(materiaPrimaId);
                
                var resultado = new
                {
                    MateriaPrimaId = materiaPrimaId,
                    NombreMateriaPrima = materiaPrima.Name,
                    StockTotal = materiaPrima.Stock,
                    CostoPromedioCalculado = costoPromedio,
                    CostoUnitarioRegistrado = materiaPrima.CostoUnitario,
                    TotalLotes = lotes.Count,
                    Lotes = lotes
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lotes de materia prima {MateriaPrimaId}", materiaPrimaId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("actualizar-precios-productos")]
        public async Task<ActionResult<object>> ActualizarPreciosProductos()
        {
            try
            {
                var productos = await _context.ProductoHydroLink
                    .Where(p => p.Activo)
                    .ToListAsync();

                var resultados = new List<object>();

                foreach (var producto in productos)
                {
                    try
                    {
                        var precioAnterior = producto.Precio;
                        var nuevoPrecio = await _precioActualizacionService.RecalcularPrecioProductoConMargenExistenteAsync(producto.Id);
                        
                        resultados.Add(new
                        {
                            ProductoId = producto.Id,
                            NombreProducto = producto.Nombre,
                            PrecioAnterior = precioAnterior,
                            NuevoPrecio = nuevoPrecio,
                            Diferencia = nuevoPrecio - precioAnterior,
                            PorcentajeCambio = precioAnterior > 0 ? ((nuevoPrecio - precioAnterior) / precioAnterior) * 100 : 0,
                            Actualizado = true
                        });

                        _logger.LogInformation(
                            "Precio actualizado para producto {ProductoId}: {PrecioAnterior} -> {NuevoPrecio}",
                            producto.Id, precioAnterior, nuevoPrecio);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error al actualizar precio del producto {ProductoId}", producto.Id);
                        
                        resultados.Add(new
                        {
                            ProductoId = producto.Id,
                            NombreProducto = producto.Nombre,
                            PrecioAnterior = producto.Precio,
                            NuevoPrecio = producto.Precio,
                            Diferencia = 0,
                            PorcentajeCambio = 0,
                            Actualizado = false,
                            Error = ex.Message
                        });
                    }
                }

                var resumen = new
                {
                    TotalProductos = productos.Count,
                    ProductosActualizados = resultados.Count(r => (bool)r.GetType().GetProperty("Actualizado")?.GetValue(r)!),
                    ProductosConError = resultados.Count(r => !(bool)r.GetType().GetProperty("Actualizado")?.GetValue(r)!),
                    FechaActualizacion = DateTime.UtcNow,
                    Detalles = resultados
                };

                return Ok(resumen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar precios de productos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("reporte-productos")]
        public async Task<ActionResult<object>> GetReporteCostosProductos()
        {
            try
            {
                var productos = await _context.ProductoHydroLink
                    .Where(p => p.Activo)
                    .Include(p => p.ComponentesRequeridos)
                        .ThenInclude(cr => cr.Componente)
                    .ToListAsync();

                var reporte = new List<object>();

                foreach (var producto in productos)
                {
                    var costoTotal = 0m;
                    foreach (var componenteRequerido in producto.ComponentesRequeridos)
                    {
                        var costoComponente = await _costoPromedioService.CalcularCostoPromedioComponenteAsync(componenteRequerido.ComponenteId);
                        costoTotal += costoComponente * componenteRequerido.Cantidad;
                    }

                    var margen = producto.Precio - costoTotal;
                    var porcentajeMargen = costoTotal > 0 ? (margen / costoTotal) * 100 : 0;

                    reporte.Add(new
                    {
                        ProductoId = producto.Id,
                        Nombre = producto.Nombre,
                        Categoria = producto.Categoria,
                        PrecioVenta = producto.Precio,
                        CostoCalculado = costoTotal,
                        MargenPesos = margen,
                        MargenPorcentaje = porcentajeMargen,
                        ComponentesRequeridos = producto.ComponentesRequeridos.Count,
                        EstadoMargen = porcentajeMargen < 20 ? "Bajo" : porcentajeMargen > 50 ? "Alto" : "Normal"
                    });
                }

                var resumen = new
                {
                    TotalProductos = productos.Count,
                    PromedioMargen = reporte.Any() ? reporte.Average(r => (decimal)r.GetType().GetProperty("MargenPorcentaje")?.GetValue(r)!) : 0,
                    ProductosMargenBajo = reporte.Count(r => r.GetType().GetProperty("EstadoMargen")?.GetValue(r)?.ToString() == "Bajo"),
                    ProductosMargenAlto = reporte.Count(r => r.GetType().GetProperty("EstadoMargen")?.GetValue(r)?.ToString() == "Alto"),
                    FechaReporte = DateTime.UtcNow,
                    Productos = reporte
                };

                return Ok(resumen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de costos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
