using HydroLink.Data;
using HydroLink.Dtos;
using HydroLink.Models;
using HydroLink.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HydroLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IInventarioService _inventarioService;
        private readonly ICostoPromedioService _costoPromedioService; 

        public VentasController(
            AppDbContext context, 
            IInventarioService inventarioService,
            ICostoPromedioService costoPromedioService) 
        {
            _context = context;
            _inventarioService = inventarioService;
            _costoPromedioService = costoPromedioService; 
        }

        // POST: api/ventas
        [HttpPost]
        [AllowAnonymous] 
        public async Task<IActionResult> CrearVenta([FromBody] VentaCreateDto createDto)
        {
            if (createDto == null)
                return BadRequest("Datos de venta inválidos");

            var producto = await _context.ProductoHydroLink
                .Include(p => p.ComponentesRequeridos)
                .ThenInclude(cr => cr.Componente)
                .FirstOrDefaultAsync(p => p.Id == createDto.ProductoId);

            if (producto == null)
                return NotFound($"Producto con ID {createDto.ProductoId} no encontrado");

            var componentesInsuficientes = new List<string>();
            
            foreach (var componenteRequerido in producto.ComponentesRequeridos)
            {
                var cantidadRequerida = componenteRequerido.Cantidad * createDto.Cantidad;
                var existenciaActual = await _inventarioService.ObtenerExistenciaAsync(componenteRequerido.ComponenteId);
                
                if (existenciaActual < cantidadRequerida)
                {
                    componentesInsuficientes.Add(
                        $"{componenteRequerido.Componente?.Nombre ?? "N/A"} (Necesarios: {cantidadRequerida}, Disponibles: {existenciaActual})");
                }
            }
            
            if (componentesInsuficientes.Any())
            {
                return BadRequest(new
                {
                    mensaje = "Inventario insuficiente para procesar la venta",
                    componentesInsuficientes = componentesInsuficientes
                });
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                var venta = new Venta
                {
                    ClienteId = createDto.ClienteId,
                    ProductoId = createDto.ProductoId,
                    Fecha = DateTime.UtcNow,
                    Cantidad = createDto.Cantidad,
                    PrecioUnitario = producto.Precio,
                    Total = producto.Precio * createDto.Cantidad,
                    Estado = "COMPLETADA",
                    Observaciones = createDto.Observaciones
                };

                _context.Venta.Add(venta);
                await _context.SaveChangesAsync();
                foreach (var componenteRequerido in producto.ComponentesRequeridos)
                {
                    var cantidadAReducir = componenteRequerido.Cantidad * createDto.Cantidad;
                    await _inventarioService.ReducirInventarioAsync(componenteRequerido.ComponenteId, cantidadAReducir);
                }

                await transaction.CommitAsync();
                
                return CreatedAtAction(nameof(GetVenta), new { id = venta.Id }, venta);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = "Error al procesar la venta",
                    detalle = ex.Message
                });
            }
        }

        // GET: api/ventas/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVenta(int id)
        {
            var venta = await _context.Venta
                .Include(v => v.Cliente)
                .Include(v => v.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
                return NotFound($"Venta con ID {id} no encontrada");

            return Ok(venta);
        }

        // GET: api/ventas
        [HttpGet]
        public async Task<IActionResult> GetVentas()
        {
            var ventas = await _context.Venta
                .Include(v => v.Cliente)
                .Include(v => v.Producto)
                .OrderByDescending(v => v.Fecha)
                .Select(v => new
                {
                    v.Id,
                    v.ClienteId,
                    v.ProductoId,
                    v.Fecha,
                    v.Cantidad,
                    v.PrecioUnitario,
                    v.Total,
                    v.Estado,
                    v.Observaciones,
                    Cliente = v.Cliente != null ? new
                    {
                        v.Cliente.Id,
                        Nombre = v.Cliente.Nombre + " " + v.Cliente.Apellido,
                        v.Cliente.Email
                    } : null,
                    Producto = v.Producto != null ? new
                    {
                        v.Producto.Id,
                        v.Producto.Nombre,
                        v.Producto.Precio
                    } : null
                })
                .ToListAsync();

            return Ok(ventas);
        }

        // GET: api/ventas/user/{email}
        [HttpGet("user/{email}")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetVentasByUser(string email)
        {
            try
            {
                
                var cliente = await _context.Persona.OfType<Cliente>()
                    .FirstOrDefaultAsync(c => c.Email == email);

                Console.WriteLine($"Cliente encontrado: {cliente != null}");
                if (cliente != null)
                {
                    Console.WriteLine($"Cliente ID: {cliente.Id}, Nombre: {cliente.Nombre} {cliente.Apellido}");
                }

                if (cliente == null)
                {
                    Console.WriteLine("Cliente no encontrado, devolviendo lista vacía");
                    return Ok(new List<object>()); 
                }

                var ventas = await _context.Venta
                    .Include(v => v.Cliente)
                    .Include(v => v.Producto)
                    .Where(v => v.ClienteId == cliente.Id)
                    .OrderByDescending(v => v.Fecha)
                    .Select(v => new
                    {
                        v.Id,
                        v.ClienteId,
                        v.ProductoId,
                        v.Fecha,
                        v.Cantidad,
                        v.PrecioUnitario,
                        v.Total,
                        v.Estado,
                        v.Observaciones,
                        Cliente = new
                        {
                            v.Cliente.Id,
                            Nombre = v.Cliente.Nombre + " " + v.Cliente.Apellido,
                            v.Cliente.Email
                        },
                        Producto = new
                        {
                            v.Producto.Id,
                            v.Producto.Nombre,
                            v.Producto.Precio,
                            v.Producto.Descripcion,
                            v.Producto.Categoria
                        }
                    })
                    .ToListAsync();

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al obtener las compras del usuario", detalle = ex.Message });
            }
        }

        // GET: api/ventas/metrics
        [HttpGet("metrics")]
        public async Task<IActionResult> GetSalesMetrics()
        {
            try
            {
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;
                var last30Days = DateTime.UtcNow.AddDays(-30);
                var monthlyRevenue = await _context.Venta
                    .Where(v => v.Fecha.Month == currentMonth && v.Fecha.Year == currentYear && v.Estado == "COMPLETADA")
                    .SumAsync(v => v.Total);

                var ordersCompleted = await _context.Venta
                    .CountAsync(v => v.Fecha.Month == currentMonth && v.Fecha.Year == currentYear && v.Estado == "COMPLETADA");
                var recentSales = await _context.Venta
                    .Where(v => v.Fecha >= last30Days && v.Estado == "COMPLETADA")
                    .ToListAsync();
                    
                var averageOrderValue = recentSales.Any() ? recentSales.Average(v => v.Total) : 0;

                var customerSatisfaction = 4.8;

                var metrics = new
                {
                    monthlyRevenue = (double)monthlyRevenue,
                    ordersCompleted = ordersCompleted,
                    averageOrderValue = (double)averageOrderValue,
                    customerSatisfaction = customerSatisfaction
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al obtener métricas de ventas", detalle = ex.Message });
            }
        }

        // GET: api/ventas/debug-product/{id} 
        [HttpGet("debug-product/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugProductInventory(int id)
        {
            try
            {
                var producto = await _context.ProductoHydroLink
                    .Include(p => p.ComponentesRequeridos)
                    .ThenInclude(cr => cr.Componente)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (producto == null)
                    return NotFound($"Producto con ID {id} no encontrado");

                var componentesInfo = new List<object>();
                
                foreach (var componenteRequerido in producto.ComponentesRequeridos)
                {
                    var existenciaActual = await _inventarioService.ObtenerExistenciaAsync(componenteRequerido.ComponenteId);
                    
                    componentesInfo.Add(new
                    {
                        ComponenteId = componenteRequerido.ComponenteId,
                        NombreComponente = componenteRequerido.Componente?.Nombre ?? "N/A",
                        CantidadRequerida = componenteRequerido.Cantidad,
                        ExistenciaDisponible = existenciaActual,
                        SuficienteParaUna = existenciaActual >= componenteRequerido.Cantidad
                    });
                }

                return Ok(new
                {
                    ProductoId = producto.Id,
                    NombreProducto = producto.Nombre,
                    ComponentesRequeridos = componentesInfo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al obtener información de inventario", detalle = ex.Message });
            }
        }

        // GET: api/ventas/seed 
        [HttpGet("seed")]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                var existingVentas = await _context.Venta.AnyAsync();
                if (existingVentas)
                {
                    return Ok(new { mensaje = "Los datos de prueba ya existen" });
                }

                var clientes = new List<Cliente>();
                if (!await _context.Persona.OfType<Cliente>().AnyAsync())
                {
                    clientes = new List<Cliente>
                    {
                        new Cliente { Nombre = "Test", Apellido = "User", Email = "test@hydrolink.com", Telefono = "555-0001" },
                        new Cliente { Nombre = "Emily", Apellido = "White", Email = "emily@example.com", Telefono = "555-0002" },
                        new Cliente { Nombre = "Robert", Apellido = "Brown", Email = "robert@example.com", Telefono = "555-0003" },
                        new Cliente { Nombre = "Lisa", Apellido = "Green", Email = "lisa@example.com", Telefono = "555-0004" },
                        new Cliente { Nombre = "David", Apellido = "Miller", Email = "david@example.com", Telefono = "555-0005" }
                    };
                    
                    _context.Persona.AddRange(clientes);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    clientes = await _context.Persona.OfType<Cliente>().Take(4).ToListAsync();
                }

                var productos = new List<ProductoHydroLink>();
                if (!await _context.ProductoHydroLink.AnyAsync())
                {
                    productos = new List<ProductoHydroLink>
                    {
                        new ProductoHydroLink { Nombre = "HydroLink Home Kit", Precio = 299.99m, Categoria = "Residencial", Descripcion = "Kit básico para riego automatizado residencial" },
                        new ProductoHydroLink { Nombre = "HydroLink Pro System", Precio = 599.99m, Categoria = "Comercial", Descripcion = "Sistema profesional de riego IoT" },
                        new ProductoHydroLink { Nombre = "HydroLink Industrial", Precio = 1299.99m, Categoria = "Industrial", Descripcion = "Sistema industrial de riego automatizado" }
                    };
                    
                    _context.ProductoHydroLink.AddRange(productos);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    productos = await _context.ProductoHydroLink.Take(3).ToListAsync();
                }

                var ventas = new List<Venta>
                {
                    new Venta
                    {
                        ClienteId = clientes[0].Id,
                        ProductoId = productos[0].Id,
                        Fecha = DateTime.UtcNow.AddDays(-3),
                        Cantidad = 1,
                        PrecioUnitario = productos[0].Precio,
                        Total = productos[0].Precio * 1,
                        Estado = "COMPLETADA",
                        Observaciones = "Entrega programada"
                    },
                    new Venta
                    {
                        ClienteId = clientes[1].Id,
                        ProductoId = productos[1].Id,
                        Fecha = DateTime.UtcNow.AddDays(-2),
                        Cantidad = 1,
                        PrecioUnitario = productos[1].Precio,
                        Total = productos[1].Precio * 1,
                        Estado = "COMPLETADA",
                        Observaciones = "Cliente premium"
                    },
                    new Venta
                    {
                        ClienteId = clientes[2].Id,
                        ProductoId = productos[2].Id,
                        Fecha = DateTime.UtcNow.AddDays(-1),
                        Cantidad = 1,
                        PrecioUnitario = productos[2].Precio,
                        Total = productos[2].Precio * 1,
                        Estado = "COMPLETADA",
                        Observaciones = "Instalación incluida"
                    },
                    new Venta
                    {
                        ClienteId = clientes[3].Id,
                        ProductoId = productos[0].Id,
                        Fecha = DateTime.UtcNow,
                        Cantidad = 2,
                        PrecioUnitario = productos[0].Precio,
                        Total = productos[0].Precio * 2,
                        Estado = "COMPLETADA",
                        Observaciones = "Descuento aplicado"
                    }
                };

                _context.Venta.AddRange(ventas);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Datos de prueba creados exitosamente", ventasCreadas = ventas.Count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al crear datos de prueba", detalle = ex.Message });
            }
        }
    }

}
