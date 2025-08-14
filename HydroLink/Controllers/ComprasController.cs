using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HydroLink.Data;
using HydroLink.Models;
using HydroLink.Dtos;
using HydroLink.Services;
using Microsoft.AspNetCore.Authorization;

namespace HydroLink.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ComprasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ComprasController> _logger;
        private readonly IPrecioActualizacionService _precioActualizacionService;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ComprasController(
            AppDbContext context, 
            ILogger<ComprasController> logger,
            IPrecioActualizacionService precioActualizacionService,
            IDbContextFactory<AppDbContext> contextFactory)
        {
            _context = context;
            _logger = logger;
            _precioActualizacionService = precioActualizacionService;
            _contextFactory = contextFactory;
        }

        // GET: api/Compras
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetalleCompraDto>>> GetCompras()
        {
            try
            {
                var compras = await _context.Compra
                    .Include(c => c.Detalles)
                    .ThenInclude(d => d.MateriaPrima)
                    .Include(c => c.Proveedor)
                    .Select(c => new DetalleCompraDto
                    {
                        Id = c.Id,
                        Fecha = c.Fecha,
                        ProveedorId = c.ProveedorId,
                        NombreProveedor = c.Proveedor.Nombre + " " + c.Proveedor.Apellido,
                        EmpresaProveedor = c.Proveedor.Empresa,
                        Total = c.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
                        CantidadItems = c.Detalles.Count,
                        Detalles = c.Detalles.Select(d => new CompraDetalleInfo
                        {
                            Id = d.Id,
                            MateriaPrimaId = d.MateriaPrimaId,
                            NombreMateriaPrima = d.MateriaPrima.Name,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Cantidad * d.PrecioUnitario
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(compras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las compras.");
                return BadRequest("Error al obtener la compras");
            }
        }

        // GET: api/Compras/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DetalleCompraDto>> GetCompra(int id)
        {
            try
            {
                var compra = await _context.Compra
                    .Include(c => c.Detalles)
                    .ThenInclude(d => d.MateriaPrima)
                    .Include(c => c.Proveedor)
                    .Where(c => c.Id == id)
                    .Select(c => new DetalleCompraDto
                    {
                        Id = c.Id,
                        Fecha = c.Fecha,
                        ProveedorId = c.ProveedorId,
                        NombreProveedor = c.Proveedor.Nombre + " " + c.Proveedor.Apellido,
                        EmpresaProveedor = c.Proveedor.Empresa,
                        Total = c.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
                        CantidadItems = c.Detalles.Count,
                        Detalles = c.Detalles.Select(d => new CompraDetalleInfo
                        {
                            Id = d.Id,
                            MateriaPrimaId = d.MateriaPrimaId,
                            NombreMateriaPrima = d.MateriaPrima.Name,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Cantidad * d.PrecioUnitario
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (compra == null)
                {
                    return NotFound(new { message = $"Compra con ID {id} no encontrada" });
                }

                return Ok(compra);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la compra con ID: {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // POST: api/Compras
        [HttpPost]
        public async Task<ActionResult<DetalleCompraDto>> NuevaCompra(CompraCreateDto compraDto)
        {
        using var transaction = await _context.Database.BeginTransactionAsync();
        _context.Database.SetCommandTimeout(60);

        try
        {
            if (compraDto == null || compraDto.Detalles == null || !compraDto.Detalles.Any())
            {
                return BadRequest("La compra debe tener al menos un detalle.");
            }

            var proveedor = await _context.Proveedor.FindAsync(compraDto.ProveedorId);
            if (proveedor == null)
            {
                return BadRequest("Proveedor no válido.");
            }

            var materiasPrimaIds = compraDto.Detalles.Select(d => d.MateriaPrimaId).ToList();
            var materiasDict = await _context.MateriaPrima
                .Where(mp => materiasPrimaIds.Contains(mp.Id))
                .ToDictionaryAsync(mp => mp.Id, mp => mp);

            var materiasNoEncontradas = materiasPrimaIds.Where(id => !materiasDict.ContainsKey(id)).ToList();
            if (materiasNoEncontradas.Any())
            {
                return BadRequest($"Materias primas no encontradas: {string.Join(", ", materiasNoEncontradas)}");
            }

            var compra = new Compra
            {
                Fecha = DateTime.UtcNow,
                ProveedorId = proveedor.Id,
                Detalles = new List<CompraDetalle>()
            };
            _context.Compra.Add(compra);
            await _context.SaveChangesAsync();

            var materiasActualizadas = new List<(int Id, decimal NuevoStock, decimal NuevoCosto)>();
            var lotesInventario = new List<LoteInventario>();
            
            foreach (var detalleDto in compraDto.Detalles)
            {
                var materia = materiasDict[detalleDto.MateriaPrimaId];
                var stockAnterior = materia.Stock;
                var costoAnterior = materia.CostoUnitario;
                var cantidadNueva = detalleDto.Cantidad;
                var costoNuevo = detalleDto.PrecioUnitario;

                var nuevoStock = stockAnterior + cantidadNueva;
                var nuevoCostoPromedio = nuevoStock > 0 ? 
                    (stockAnterior * costoAnterior + cantidadNueva * costoNuevo) / nuevoStock : costoNuevo;

                materia.Stock = nuevoStock;
                materia.CostoUnitario = nuevoCostoPromedio;

                var detalle = new CompraDetalle
                {
                    CompraId = compra.Id,
                    MateriaPrimaId = materia.Id,
                    Cantidad = detalleDto.Cantidad,
                    PrecioUnitario = detalleDto.PrecioUnitario
                };

                compra.Detalles.Add(detalle);

                var loteInventario = new LoteInventario
                {
                    MateriaPrimaId = detalle.MateriaPrimaId,
                    CompraId = compra.Id,
                    ProveedorId = proveedor.Id,
                    CantidadInicial = detalle.Cantidad,
                    CantidadDisponible = detalle.Cantidad,
                    CostoUnitario = detalle.PrecioUnitario,
                    CostoTotal = detalle.Cantidad * detalle.PrecioUnitario,
                    FechaIngreso = DateTime.UtcNow,
                    NumeroLote = $"L{compra.Id:000}-{detalle.MateriaPrimaId:000}-{DateTime.UtcNow:yyyyMMdd}",
                    ActualizadoPor = "Sistema"
                };
                lotesInventario.Add(loteInventario);

                if (Math.Abs(costoAnterior - nuevoCostoPromedio) > 0.01m)
                {
                    materiasActualizadas.Add((materia.Id, nuevoStock, nuevoCostoPromedio));
                }
            }

            _context.LoteInventario.AddRange(lotesInventario);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Compra {CompraId} procesada exitosamente con {DetallesCount} detalles", 
                compra.Id, compra.Detalles.Count);

            try
            {
                foreach (var (materiaPrimaId, _, nuevoCosto) in materiasActualizadas)
                {
                    await _precioActualizacionService.ActualizarPreciosProductosPorMateriaPrimaAsync(
                        materiaPrimaId, nuevoCosto);
                    _logger.LogInformation(
                        "Precios actualizados SINCRÓNICAMENTE para materia prima {MateriaPrimaId} con nuevo costo {NuevoCosto}", 
                        materiaPrimaId, nuevoCosto);
                }
                
                _logger.LogInformation("Todos los precios de productos actualizados exitosamente para compra {CompraId}", compra.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en actualización síncrona de precios para compra {CompraId}", compra.Id);
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    foreach (var detalle in compra.Detalles)
                    {
                        await GenerarMovimientosComponentesParaMateriaPrima(
                            detalle.MateriaPrimaId, detalle.Cantidad, detalle.PrecioUnitario);
                    }
                    _logger.LogInformation("Movimientos de componentes generados en background para compra {CompraId}", compra.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al generar movimientos de componentes en background");
                }
            });

            var compraCreada = new DetalleCompraDto
            {
                Id = compra.Id,
                Fecha = compra.Fecha,
                ProveedorId = compra.ProveedorId,
                NombreProveedor = proveedor.Nombre + " " + proveedor.Apellido,
                EmpresaProveedor = proveedor.Empresa,
                Total = compra.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
                CantidadItems = compra.Detalles.Count,
                Detalles = compra.Detalles.Select(d => new CompraDetalleInfo
                {
                    Id = d.Id,
                    MateriaPrimaId = d.MateriaPrimaId,
                    NombreMateriaPrima = _context.MateriaPrima.Find(d.MateriaPrimaId)?.Name ?? "Desconocido",
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Cantidad * d.PrecioUnitario
                }).ToList()
            };

            return CreatedAtAction(nameof(GetCompra), new { id = compra.Id }, compraCreada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al confirmar la compra.");
            return BadRequest("No se pudo realizar la compra.");
        }
    }


        [HttpGet("proveedor/{proveedorId}")]
        public async Task<ActionResult<IEnumerable<DetalleCompraDto>>> GetComprasPorProveedor(int proveedorId)
        {
            try
            {
                var proveedor = await _context.Proveedor.FindAsync(proveedorId);
                if (proveedor == null)
                {
                    return NotFound(new { message = $"Proveedor con ID {proveedorId} no encontrado" });
                }

                var compras = await _context.Compra
                    .Include(c => c.Detalles)
                    .ThenInclude(d => d.MateriaPrima)
                    .Include(c => c.Proveedor)
                    .Where(c => c.ProveedorId == proveedorId)
                    .Select(c => new DetalleCompraDto
                    {
                        Id = c.Id,
                        Fecha = c.Fecha,
                        ProveedorId = c.ProveedorId,
                        NombreProveedor = c.Proveedor.Nombre + " " + c.Proveedor.Apellido,
                        EmpresaProveedor = c.Proveedor.Empresa,
                        Total = c.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
                        CantidadItems = c.Detalles.Count,
                        Detalles = c.Detalles.Select(d => new CompraDetalleInfo
                        {
                            Id = d.Id,
                            MateriaPrimaId = d.MateriaPrimaId,
                            NombreMateriaPrima = d.MateriaPrima.Name,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Cantidad * d.PrecioUnitario
                        }).ToList()
                    })
                    .OrderByDescending(c => c.Fecha)
                    .ToListAsync();

                return Ok(compras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener compras del proveedor {ProveedorId}", proveedorId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("fecha")]
        public async Task<ActionResult<IEnumerable<DetalleCompraDto>>> GetComprasPorFecha(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    return BadRequest("La fecha de inicio no puede ser mayor que la fecha fin");
                }

                var compras = await _context.Compra
                    .Include(c => c.Detalles)
                    .ThenInclude(d => d.MateriaPrima)
                    .Include(c => c.Proveedor)
                    .Where(c => c.Fecha >= fechaInicio && c.Fecha <= fechaFin)
                    .Select(c => new DetalleCompraDto
                    {
                        Id = c.Id,
                        Fecha = c.Fecha,
                        ProveedorId = c.ProveedorId,
                        NombreProveedor = c.Proveedor.Nombre + " " + c.Proveedor.Apellido,
                        EmpresaProveedor = c.Proveedor.Empresa,
                        Total = c.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
                        CantidadItems = c.Detalles.Count
                    })
                    .OrderByDescending(c => c.Fecha)
                    .ToListAsync();

                return Ok(compras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener compras por fecha");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("estadisticas")]
        public async Task<ActionResult<object>> GetEstadisticasCompras()
        {
            try
            {
                var compras = await _context.Compra
                    .Include(c => c.Detalles)
                    .ToListAsync();

                var totalCompras = compras.Count;
                var montoTotalCompras = compras.Sum(c => c.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario));
                var promedioCompra = totalCompras > 0 ? montoTotalCompras / totalCompras : 0;
                var comprasPorMes = compras
                    .GroupBy(c => new { c.Fecha.Year, c.Fecha.Month })
                    .Select(g => new
                    {
                        Año = g.Key.Year,
                        Mes = g.Key.Month,
                        CantidadCompras = g.Count(),
                        MontoTotal = g.Sum(c => c.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario))
                    })
                    .OrderByDescending(x => x.Año).ThenByDescending(x => x.Mes)
                    .Take(12)
                    .ToList();

                var estadisticas = new
                {
                    TotalCompras = totalCompras,
                    MontoTotalCompras = montoTotalCompras,
                    PromedioCompra = promedioCompra,
                    ComprasPorMes = comprasPorMes
                };

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de compras");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<DetalleCompraDto>> EditarCompra(int id, CompraCreateDto compraDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (compraDto == null || compraDto.Detalles == null || !compraDto.Detalles.Any())
                {
                    return BadRequest("La compra debe tener al menos un detalle.");
                }

                var compraExistente = await _context.Compra
                    .Include(c => c.Detalles)
                        .ThenInclude(d => d.MateriaPrima)
                    .Include(c => c.Proveedor)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (compraExistente == null)
                {
                    return NotFound(new { message = $"Compra con ID {id} no encontrada" });
                }

                var proveedor = await _context.Proveedor.FindAsync(compraDto.ProveedorId);
                if (proveedor == null)
                {
                    return BadRequest("Proveedor no válido.");
                }

                foreach (var detalleAnterior in compraExistente.Detalles)
                {
                    var materia = detalleAnterior.MateriaPrima;
                    if (materia != null)
                    {
                        var movimientoReversion = new MovimientoInventario
                        {
                            MateriaPrimaId = materia.Id,
                            FechaMovimiento = DateTime.UtcNow,
                            TipoMovimiento = "Salida",
                            Cantidad = detalleAnterior.Cantidad,
                            PrecioUnitario = detalleAnterior.PrecioUnitario,
                            CostoTotal = detalleAnterior.Cantidad * detalleAnterior.PrecioUnitario,
                            NumeroLote = $"REV-{id}-{detalleAnterior.Id}",
                            Proveedor = compraExistente.Proveedor?.Empresa,
                            CompraId = id,
                            Observaciones = $"Reversión por edición de compra #{id}"
                        };
                        _context.MovimientoInventario.Add(movimientoReversion);
                        materia.Stock -= detalleAnterior.Cantidad;
                        if (materia.Stock > 0)
                        {
                            var valorTotalAnterior = (materia.Stock + detalleAnterior.Cantidad) * materia.CostoUnitario;
                            var valorDetalleAnterior = detalleAnterior.Cantidad * detalleAnterior.PrecioUnitario;
                            var nuevoValorTotal = valorTotalAnterior - valorDetalleAnterior;
                            materia.CostoUnitario = nuevoValorTotal / materia.Stock;
                        }
                        else
                        {
                            materia.CostoUnitario = 0;
                        }
                    }
                }

                _context.CompraDetalle.RemoveRange(compraExistente.Detalles);
                compraExistente.Detalles.Clear();
                compraExistente.ProveedorId = compraDto.ProveedorId;
                compraExistente.Fecha = DateTime.UtcNow;
                foreach (var detalleDto in compraDto.Detalles)
                {
                    var materiaPrima = await _context.MateriaPrima.FindAsync(detalleDto.MateriaPrimaId);
                    if (materiaPrima == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest($"La materia prima con ID {detalleDto.MateriaPrimaId} no existe.");
                    }
                    var stockAnterior = materiaPrima.Stock;
                    var costoAnterior = materiaPrima.CostoUnitario;
                    var cantidadNueva = detalleDto.Cantidad;
                    var costoNuevo = detalleDto.PrecioUnitario;

                    var nuevoStock = stockAnterior + cantidadNueva;
                    var nuevoCostoPromedio = stockAnterior > 0 
                        ? (stockAnterior * costoAnterior + cantidadNueva * costoNuevo) / nuevoStock
                        : costoNuevo;

                    materiaPrima.Stock = nuevoStock;
                    materiaPrima.CostoUnitario = nuevoCostoPromedio;
                    var nuevoDetalle = new CompraDetalle
                    {
                        CompraId = id,
                        MateriaPrimaId = detalleDto.MateriaPrimaId,
                        Cantidad = detalleDto.Cantidad,
                        PrecioUnitario = detalleDto.PrecioUnitario
                    };

                    compraExistente.Detalles.Add(nuevoDetalle);
                    var movimientoEntrada = new MovimientoInventario
                    {
                        MateriaPrimaId = materiaPrima.Id,
                        FechaMovimiento = DateTime.UtcNow,
                        TipoMovimiento = "Entrada",
                        Cantidad = cantidadNueva,
                        PrecioUnitario = costoNuevo,
                        CostoTotal = cantidadNueva * costoNuevo,
                        NumeroLote = $"EDIT-{id}-{materiaPrima.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                        Proveedor = proveedor.Empresa,
                        CompraId = id,
                        Observaciones = $"Entrada por edición de compra #{id} - {materiaPrima.Name}"
                    };
                    _context.MovimientoInventario.Add(movimientoEntrada);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                var compraActualizada = new DetalleCompraDto
                {
                    Id = compraExistente.Id,
                    Fecha = compraExistente.Fecha,
                    ProveedorId = compraExistente.ProveedorId,
                    NombreProveedor = proveedor.Nombre + " " + proveedor.Apellido,
                    EmpresaProveedor = proveedor.Empresa,
                    Total = compraExistente.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
                    CantidadItems = compraExistente.Detalles.Count,
                    Detalles = compraExistente.Detalles.Select(d => new CompraDetalleInfo
                    {
                        Id = d.Id,
                        MateriaPrimaId = d.MateriaPrimaId,
                        NombreMateriaPrima = _context.MateriaPrima.Find(d.MateriaPrimaId)?.Name ?? "Desconocido",
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Cantidad * d.PrecioUnitario
                    }).ToList()
                };

                return Ok(compraActualizada);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al editar la compra con ID: {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // DELETE: api/Compras/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompra(int id)
        {
            var compra = await _context.Compra.FindAsync(id);
            if (compra == null)
            {
                return NotFound();
            }

            _context.Compra.Remove(compra);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task GenerarMovimientosComponentesParaMateriaPrima(int materiaPrimaId, decimal cantidadComprada, decimal costoUnitario)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var componentesQueUsanMateria = await context.ComponenteMateriaPrima
                    .Where(cm => cm.MateriaPrimaId == materiaPrimaId && cm.Activo)
                    .Include(cm => cm.Componente)
                    .ToListAsync();

                foreach (var componenteMateria in componentesQueUsanMateria)
                {
                    var unidadesComponenteProducibles = cantidadComprada / componenteMateria.CantidadConMerma;   
                    if (unidadesComponenteProducibles > 0)
                    {
                        var costoComponente = componenteMateria.CantidadConMerma * costoUnitario;
                        var movimientoComponente = new MovimientoComponente
                        {
                            ComponenteId = componenteMateria.ComponenteId,
                            TipoMovimiento = "ENTRADA",
                            Cantidad = unidadesComponenteProducibles,
                            FechaMovimiento = DateTime.UtcNow,
                            PrecioUnitario = costoComponente,
                            CostoTotal = costoComponente * unidadesComponenteProducibles,
                            NumeroLote = $"COMPRA-{DateTime.UtcNow:yyyyMMddHHmmss}-MP{materiaPrimaId}",
                            Observaciones = $"Entrada automática por compra de {cantidadComprada} unidades de materia prima ID {materiaPrimaId}"
                        };

                        context.MovimientoComponente.Add(movimientoComponente);
                        
                        _logger.LogInformation(
                            "Generada entrada automática: {Cantidad} unidades del componente {ComponenteNombre} (ID: {ComponenteId}) por compra de materia prima {MateriaPrimaId}",
                            unidadesComponenteProducibles,
                            componenteMateria.Componente?.Nombre ?? "N/A",
                            componenteMateria.ComponenteId,
                            materiaPrimaId
                        );
                    }
                }
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar movimientos de componentes para materia prima {MateriaPrimaId}", materiaPrimaId);
            }
        }
    }
}
