using HydroLink.Data;
using HydroLink.Dtos;
using HydroLink.Models;
using HydroLink.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HydroLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CotizacionesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPrecioComponenteService _precioService;
        private readonly IInventarioService _inventarioService;

        public CotizacionesController(
            AppDbContext context, 
            IPrecioComponenteService precioService,
            IInventarioService inventarioService)
        {
            _context = context;
            _precioService = precioService;
            _inventarioService = inventarioService;
        }

        // POST: api/cotizaciones
        [HttpPost]
        public async Task<IActionResult> CrearCotizacion([FromBody] CotizacionCreateDto createDto)
        {
            if (createDto == null)
                return BadRequest("Datos de cotización inválidos");
            var producto = await _context.ProductoHydroLink
                .Include(p => p.ComponentesRequeridos)
                .ThenInclude(cr => cr.Componente)
                .FirstOrDefaultAsync(p => p.Id == createDto.ProductoId);

            if (producto == null)
                return NotFound($"Producto con ID {createDto.ProductoId} no encontrado");
            var cliente = await _context.Persona.FindAsync(createDto.ClienteId);

            decimal costoTotalComponentes = 0;
            
            if (createDto.ComponenteIds != null && createDto.ComponenteIds.Any())
            {
                foreach (var componenteId in createDto.ComponenteIds)
                {
                    var componenteRequerido = producto.ComponentesRequeridos
                        .FirstOrDefault(cr => cr.ComponenteId == componenteId);
                    
                    if (componenteRequerido != null)
                    {
                        var precioComponente = await _precioService.ObtenerPrecioPromedioAsync(componenteRequerido.ComponenteId);
                        costoTotalComponentes += precioComponente * componenteRequerido.Cantidad;
                    }
                }
            }
            else
            {
                foreach (var cr in producto.ComponentesRequeridos)
                {
                    var precioComponente = await _precioService.ObtenerPrecioPromedioAsync(cr.ComponenteId);
                    costoTotalComponentes += precioComponente * cr.Cantidad;
                }
            }

            var manoDeObra = costoTotalComponentes * 0.20m;
            var otrosCostos = costoTotalComponentes * 0.10m;
            var subtotal = costoTotalComponentes + manoDeObra + otrosCostos;
            var ganancia = subtotal * (createDto.PorcentajeGanancia / 100);
            var totalEstimado = subtotal + ganancia;

            var cotizacion = new Cotizacion
            {
                ClienteId = createDto.ClienteId,
                ProductoId = createDto.ProductoId,
                Fecha = DateTime.UtcNow,
                FechaVencimiento = DateTime.UtcNow.AddDays(30),
                NombreProyecto = createDto.NombreProyecto,
                Descripcion = createDto.Descripcion,
                NombreCliente = cliente?.Nombre ?? $"Cliente ID: {createDto.ClienteId}",
                EmailCliente = cliente?.Email ?? "",
                TelefonoCliente = cliente?.Telefono ?? "",
                Observaciones = createDto.Observaciones,
                SubtotalComponentes = costoTotalComponentes,
                SubtotalManoObra = manoDeObra,
                SubtotalMateriales = otrosCostos,
                PorcentajeGanancia = createDto.PorcentajeGanancia,
                MontoGanancia = ganancia,
                TotalEstimado = totalEstimado,
                Estado = "PENDIENTE",
                Detalles = new List<CotizacionDetalle>()
            };

            _context.Cotizacion.Add(cotizacion);
            await _context.SaveChangesAsync();

            var response = new
            {
                cotizacion.Id,
                cotizacion.NombreProyecto,
                cotizacion.Descripcion,
                cotizacion.Fecha,
                cotizacion.FechaVencimiento,
                cotizacion.NombreCliente,
                cotizacion.EmailCliente,
                cotizacion.TelefonoCliente,
                cotizacion.SubtotalComponentes,
                cotizacion.SubtotalManoObra,
                cotizacion.SubtotalMateriales,
                cotizacion.PorcentajeGanancia,
                cotizacion.MontoGanancia,
                cotizacion.TotalEstimado,
                cotizacion.Estado,
                cotizacion.Observaciones,
                ClienteId = cotizacion.ClienteId,
                ProductoId = cotizacion.ProductoId
            };

            return CreatedAtAction(nameof(GetCotizacion), new { id = cotizacion.Id }, response);
        }

        // GET: api/cotizaciones/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCotizacion(int id)
        {
            var cotizacion = await _context.Cotizacion
                .Include(c => c.Cliente)
                .Include(c => c.Producto)
                .Select(c => new
                {
                    c.Id,
                    c.NombreProyecto,
                    c.Descripcion,
                    c.Fecha,
                    c.FechaVencimiento,
                    c.NombreCliente,
                    c.EmailCliente,
                    c.TelefonoCliente,
                    c.SubtotalComponentes,
                    c.SubtotalManoObra,
                    c.SubtotalMateriales,
                    c.PorcentajeGanancia,
                    c.MontoGanancia,
                    c.TotalEstimado,
                    c.Estado,
                    c.Observaciones,
                    Cliente = new
                    {
                        c.Cliente.Id,
                        c.Cliente.Nombre,
                        c.Cliente.Apellido,
                        c.Cliente.Email,
                        c.Cliente.Telefono
                    },
                    Producto = new
                    {
                        c.Producto.Id,
                        c.Producto.Nombre,
                        c.Producto.Descripcion,
                        c.Producto.Categoria,
                        c.Producto.Precio
                    }
                })
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotizacion == null)
                return NotFound($"Cotización con ID {id} no encontrada");

            return Ok(cotizacion);
        }

        // GET: api/cotizaciones
        [HttpGet]
        public async Task<IActionResult> GetCotizaciones()
        {
            var cotizaciones = await _context.Cotizacion
                .Include(c => c.Cliente)
                .Include(c => c.Producto)
                .Include(c => c.Venta)
                .Select(c => new
                {
                    c.Id,
                    c.NombreProyecto,
                    c.Descripcion,
                    c.Fecha,
                    c.FechaVencimiento,
                    c.NombreCliente,
                    c.EmailCliente,
                    c.TelefonoCliente,
                    c.SubtotalComponentes,
                    c.SubtotalManoObra,
                    c.SubtotalMateriales,
                    c.PorcentajeGanancia,
                    c.MontoGanancia,
                    c.TotalEstimado,
                    c.Estado,
                    c.Observaciones,
                    c.VentaId,
                    Cliente = new
                    {
                        c.Cliente.Id,
                        c.Cliente.Nombre,
                        c.Cliente.Apellido,
                        c.Cliente.Email
                    },
                    Producto = new
                    {
                        c.Producto.Id,
                        c.Producto.Nombre,
                        c.Producto.Categoria,
                        c.Producto.Precio
                    },
                    Venta = c.Venta != null ? new
                    {
                        c.Venta.Id,
                        c.Venta.Fecha,
                        c.Venta.Estado,
                        c.Venta.Total,
                        c.Venta.Cantidad
                    } : null
                })
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

        return Ok(cotizaciones);
    }

    // PATCH: api/cotizaciones/{id}/status
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateQuoteStatus(int id, [FromBody] UpdateQuoteStatusDto updateDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var cotizacion = await _context.Cotizacion
                .Include(c => c.Cliente)
                .Include(c => c.Producto)
                    .ThenInclude(p => p.ComponentesRequeridos)
                        .ThenInclude(cr => cr.Componente)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (cotizacion == null)
                return NotFound($"Cotización con ID {id} no encontrada");

            var validStates = new[] { "BORRADOR", "PENDIENTE", "APROBADA", "RECHAZADA" };
            if (!validStates.Contains(updateDto.Estado))
                return BadRequest("Estado inválido");

            if (updateDto.Estado == "APROBADA" && cotizacion.Estado != "APROBADA")
            {
                if (cotizacion.VentaId != null)
                {
                    return BadRequest(new
                    {
                        mensaje = "Esta cotización ya tiene una venta asociada",
                        ventaId = cotizacion.VentaId
                    });
                }

                var componentesInsuficientes = new List<string>();
                var cantidadProducto = 1;
                
                if (cotizacion.Producto?.ComponentesRequeridos != null)
                {
                    foreach (var componenteRequerido in cotizacion.Producto.ComponentesRequeridos)
                    {
                        var cantidadRequerida = componenteRequerido.Cantidad * cantidadProducto;
                        var existenciaActual = await _inventarioService.ObtenerExistenciaAsync(componenteRequerido.ComponenteId);
                        
                        if (existenciaActual < cantidadRequerida)
                        {
                            componentesInsuficientes.Add(
                                $"{componenteRequerido.Componente?.Nombre ?? "N/A"} (Necesarios: {cantidadRequerida}, Disponibles: {existenciaActual})");
                        }
                    }
                }
                
                if (componentesInsuficientes.Any())
                {
                    return BadRequest(new
                    {
                        mensaje = "No se puede aprobar la cotización. Inventario insuficiente para procesar la venta",
                        componentesInsuficientes = componentesInsuficientes
                    });
                }

                var totalVenta = cotizacion.TotalEstimado * cantidadProducto;
                var venta = new Venta
                {
                    ClienteId = cotizacion.ClienteId,
                    ProductoId = cotizacion.ProductoId,
                    CotizacionId = cotizacion.Id,
                    Fecha = DateTime.UtcNow,
                    Cantidad = cantidadProducto,
                    PrecioUnitario = cotizacion.TotalEstimado,
                    Total = totalVenta,
                    Estado = "PENDIENTE",
                    Observaciones = $"Venta generada automáticamente desde cotización #{cotizacion.Id} - {cotizacion.NombreProyecto}"
                };

                _context.Venta.Add(venta);
                await _context.SaveChangesAsync();

                cotizacion.VentaId = venta.Id;
                
                if (cotizacion.Producto?.ComponentesRequeridos != null)
                {
                    foreach (var componenteRequerido in cotizacion.Producto.ComponentesRequeridos)
                    {
                        var cantidadAReducir = componenteRequerido.Cantidad * cantidadProducto;
                        await _inventarioService.ReducirInventarioAsync(componenteRequerido.ComponenteId, cantidadAReducir);
                    }
                }
                venta.Estado = "COMPLETADA";
                await _context.SaveChangesAsync();
            }

            cotizacion.Estado = updateDto.Estado;
            cotizacion.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = new 
            { 
                message = "Estado actualizado correctamente", 
                estado = cotizacion.Estado,
                ventaCreada = updateDto.Estado == "APROBADA" && cotizacion.VentaId != null,
                ventaId = cotizacion.VentaId
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new 
            { 
                mensaje = "Error al actualizar el estado de la cotización", 
                detalle = ex.Message 
            });
        }
    }
}

public class UpdateQuoteStatusDto
{
    public string Estado { get; set; } = string.Empty;
}

    public class CotizacionCreateDto
    {
        public int ClienteId { get; set; }
        public int ProductoId { get; set; }
        public string NombreProyecto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public decimal PorcentajeGanancia { get; set; } = 25.0m; 
        public List<int> ComponenteIds { get; set; } = new List<int>();
    }
}
