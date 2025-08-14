using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HydroLink.Data;
using HydroLink.Dtos;
using HydroLink.Models;
using Microsoft.AspNetCore.Authorization;

namespace HydroLink.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InventarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InventarioController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Inventario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventarioMateriaPrimaDto>>> GetInventario()
        {
            var materiasPrimas = await _context.MateriaPrima
                .Include(mp => mp.Compras)
                    .ThenInclude(cd => cd.Compra)
                        .ThenInclude(c => c.Proveedor)
                .ToListAsync();

            var inventarioDto = materiasPrimas.Select(mp => new InventarioMateriaPrimaDto
            {
                Id = mp.Id,
                Nombre = mp.Name,
                UnidadMedida = mp.UnidadMedida,
                StockActual = mp.Stock,
                CostoUnitarioPromedio = mp.CostoUnitario,
                ValorTotalInventario = mp.Stock * mp.CostoUnitario,
                StockMinimo = 10,
                StockMaximo = 1000,
                EstadoStock = GetEstadoStock(mp.Stock, 10, 1000),
                FechaUltimaCompra = mp.Compras
                    .OrderByDescending(cd => cd.Compra.Fecha)
                    .FirstOrDefault()?.Compra.Fecha,
                UltimoPrecioCompra = mp.Compras
                    .OrderByDescending(cd => cd.Compra.Fecha)
                    .FirstOrDefault()?.PrecioUnitario,
                MovimientosRecientes = GetMovimientosRecientes(mp)
            }).ToList();

            return Ok(inventarioDto);
        }

        // GET: api/Inventario/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<InventarioMateriaPrimaDto>> GetInventario(int id)
        {
            var materiaPrima = await _context.MateriaPrima
                .Include(mp => mp.Compras)
                    .ThenInclude(cd => cd.Compra)
                        .ThenInclude(c => c.Proveedor)
                .FirstOrDefaultAsync(mp => mp.Id == id);

            if (materiaPrima == null)
            {
                return NotFound();
            }

            var inventarioDto = new InventarioMateriaPrimaDto
            {
                Id = materiaPrima.Id,
                Nombre = materiaPrima.Name,
                UnidadMedida = materiaPrima.UnidadMedida,
                StockActual = materiaPrima.Stock,
                CostoUnitarioPromedio = materiaPrima.CostoUnitario,
                ValorTotalInventario = materiaPrima.Stock * materiaPrima.CostoUnitario,
                StockMinimo = 10,
                StockMaximo = 1000,
                EstadoStock = GetEstadoStock(materiaPrima.Stock, 10, 1000),
                FechaUltimaCompra = materiaPrima.Compras
                    .OrderByDescending(cd => cd.Compra.Fecha)
                    .FirstOrDefault()?.Compra.Fecha,
                UltimoPrecioCompra = materiaPrima.Compras
                    .OrderByDescending(cd => cd.Compra.Fecha)
                    .FirstOrDefault()?.PrecioUnitario,
                MovimientosRecientes = GetMovimientosRecientes(materiaPrima)
            };

            return Ok(inventarioDto);
        }

        // GET: api/Inventario/resumen
        [HttpGet("resumen")]
        public async Task<ActionResult<ResumenInventarioDto>> GetResumenInventario()
        {
            var materiasPrimas = await _context.MateriaPrima.ToListAsync();

            var totalMateriasPrimas = materiasPrimas.Count;
            var valorTotalInventario = materiasPrimas.Sum(mp => mp.Stock * mp.CostoUnitario);
            var stockMinimo = 10;
            var stockCritico = 5;

            var materialesBajoStock = materiasPrimas.Where(mp => mp.Stock <= stockMinimo && mp.Stock > stockCritico).ToList();
            var materialesStockCritico = materiasPrimas.Where(mp => mp.Stock <= stockCritico).ToList();

            var resumen = new ResumenInventarioDto
            {
                TotalMateriasPrimas = totalMateriasPrimas,
                ValorTotalInventario = valorTotalInventario,
                MaterialesBajoStock = materialesBajoStock.Count,
                MaterialesStockCritico = materialesStockCritico.Count,
                LotesPorVencer = 0,
                LotesVencidos = 0,
                MaterialesCriticos = materialesStockCritico.Select(mp => new MaterialBajoStockDto
                {
                    Id = mp.Id,
                    Nombre = mp.Name,
                    StockActual = mp.Stock,
                    StockMinimo = stockMinimo,
                    EstadoStock = GetEstadoStock(mp.Stock, stockMinimo, 1000)
                }).ToList()
            };

            return Ok(resumen);
        }

        // POST: api/Inventario/ajustar
        [HttpPost("ajustar")]
        public async Task<ActionResult> AjustarInventario([FromBody] AjusteInventarioDto ajuste)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var materiaPrima = await _context.MateriaPrima
                    .FirstOrDefaultAsync(mp => mp.Id == ajuste.MateriaPrimaId);

                if (materiaPrima == null)
                {
                    return NotFound("Materia prima no encontrada");
                }

                var nuevoStock = materiaPrima.Stock + ajuste.CantidadAjuste;
                if (nuevoStock < 0)
                {
                    return BadRequest($"El ajuste resultaría en stock negativo. Stock actual: {materiaPrima.Stock}");
                }

                materiaPrima.Stock = nuevoStock;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { 
                    Mensaje = "Ajuste realizado correctamente",
                    StockAnterior = materiaPrima.Stock - ajuste.CantidadAjuste,
                    StockNuevo = materiaPrima.Stock,
                    CantidadAjustada = ajuste.CantidadAjuste
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al realizar el ajuste: {ex.Message}");
            }
        }

        // GET: api/Inventario/bajo-stock
        [HttpGet("bajo-stock")]
        public async Task<ActionResult<IEnumerable<MaterialBajoStockDto>>> GetMaterialesBajoStock()
        {
            var stockMinimo = 10;
            var stockCritico = 5;

            var materialesBajoStock = await _context.MateriaPrima
                .Where(mp => mp.Stock <= stockMinimo)
                .Select(mp => new MaterialBajoStockDto
                {
                    Id = mp.Id,
                    Nombre = mp.Name,
                    StockActual = mp.Stock,
                    StockMinimo = stockMinimo,
                    EstadoStock = mp.Stock <= stockCritico ? "Crítico" : "Bajo"
                })
                .OrderBy(mp => mp.StockActual)
                .ToListAsync();

            return Ok(materialesBajoStock);
        }

        // GET: api/Inventario/movimientos/{id}
        [HttpGet("movimientos/{id}")]
        public async Task<ActionResult<IEnumerable<MovimientoInventarioDto>>> GetMovimientosMateriaPrima(int id)
        {
            var materiaPrima = await _context.MateriaPrima
                .Include(mp => mp.Compras)
                    .ThenInclude(cd => cd.Compra)
                        .ThenInclude(c => c.Proveedor)
                .FirstOrDefaultAsync(mp => mp.Id == id);

            if (materiaPrima == null)
            {
                return NotFound("Materia prima no encontrada");
            }

            var movimientos = GetMovimientosRecientes(materiaPrima);
            return Ok(movimientos);
        }
        private string GetEstadoStock(int stockActual, int stockMinimo, int stockMaximo)
        {
            if (stockActual <= 5) return "Crítico";
            if (stockActual <= stockMinimo) return "Bajo";
            if (stockActual >= stockMaximo) return "Alto";
            return "Normal";
        }

        private List<MovimientoInventarioDto> GetMovimientosRecientes(MateriaPrima materiaPrima)
        {
            var movimientos = new List<MovimientoInventarioDto>();

            if (materiaPrima.Compras?.Any() == true)
            {
                foreach (var detalle in materiaPrima.Compras.OrderByDescending(cd => cd.Compra?.Fecha ?? DateTime.MinValue).Take(10))
                {
                    if (detalle.Compra != null)
                    {
                        movimientos.Add(new MovimientoInventarioDto
                        {
                            Id = detalle.Id,
                            Fecha = detalle.Compra.Fecha,
                            TipoMovimiento = "Entrada",
                            Cantidad = detalle.Cantidad,
                            PrecioUnitario = detalle.PrecioUnitario,
                            Concepto = "Compra",
                            Proveedor = detalle.Compra.Proveedor != null ? 
                                $"{detalle.Compra.Proveedor.Nombre} {detalle.Compra.Proveedor.Apellido}" : 
                                "Proveedor no especificado",
                            CompraId = detalle.CompraId
                        });
                    }
                }
            }

            return movimientos.OrderByDescending(m => m.Fecha).ToList();
        }
    }
}
