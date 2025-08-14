using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HydroLink.Services;
using HydroLink.Data;
using HydroLink.Models;
using Microsoft.EntityFrameworkCore;

namespace HydroLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly ILogger<PurchaseController> _logger;
        private readonly HydroLink.Services.IInventarioService _inventarioService;
        private readonly AppDbContext _context;

        public PurchaseController(ILogger<PurchaseController> logger, HydroLink.Services.IInventarioService inventarioService, AppDbContext context)
        {
            _logger = logger;
            _inventarioService = inventarioService;
            _context = context;
        }
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPurchase([FromBody] PurchaseProcessRequest request)
        {
            _logger.LogInformation("=== INICIANDO PROCESO DE COMPRA OPTIMIZADO ===");
            
            try
            {
                if (request == null || request.Pedido == null)
                {
                    _logger.LogError("Datos de compra inválidos: request o Pedido es null");
                    return BadRequest(new { mensaje = "Datos de compra inválidos" });
                }
                
                if (string.IsNullOrEmpty(request.Tarjeta?.NumeroTarjeta) || request.Tarjeta.NumeroTarjeta.Length < 13)
                {
                    return BadRequest(new { mensaje = "Número de tarjeta inválido" });
                }
                
                if (string.IsNullOrEmpty(request.Cliente?.Email) || !request.Cliente.Email.Contains("@"))
                {
                    return BadRequest(new { mensaje = "Email del cliente inválido" });
                }

                _logger.LogInformation($"Procesando compra para producto ID: {request.Pedido.ProductoId}, Email: {request.Cliente.Email}");

                _context.Database.SetCommandTimeout(30); 
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var producto = await _context.ProductoHydroLink
                        .FirstOrDefaultAsync(p => p.Id == request.Pedido.ProductoId && p.Activo);
                    
                    var usuario = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == request.Cliente.Email);
                    
                    var cliente = await _context.Persona.OfType<Cliente>()
                        .FirstOrDefaultAsync(c => c.Email == request.Cliente.Email);
                    
                    if (producto == null)
                    {
                        _logger.LogError($"Producto no encontrado con ID: {request.Pedido.ProductoId}");
                        return BadRequest(new { mensaje = "Producto no encontrado o no disponible" });
                    }
                    
                    _logger.LogInformation($"Producto: {producto.Nombre}, PDF: {!string.IsNullOrEmpty(producto.ManualUsuarioPdf)}");

                    if (cliente == null)
                    {
                        _logger.LogInformation("Creando nuevo cliente");
                        cliente = new Cliente
                        {
                            Nombre = request.Cliente.Nombre,
                            Apellido = request.Cliente.Apellido,
                            Email = request.Cliente.Email,
                            Telefono = request.Cliente.Telefono,
                            Direccion = request.Cliente.Direccion
                        };
                        _context.Persona.Add(cliente);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Cliente creado con ID: {cliente.Id}");
                    }
                    else
                    {
                        _logger.LogInformation($"Cliente existente encontrado con ID: {cliente.Id}");
                    }

                    var venta = new Venta
                    {
                        ClienteId = cliente.Id,
                        ProductoId = request.Pedido.ProductoId,
                        Fecha = DateTime.UtcNow,
                        Cantidad = request.Pedido.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Total = request.Pedido.Total,
                        Estado = "COMPLETADA",
                        Observaciones = request.Pedido.Observaciones ?? "Compra online"
                    };
                    
                    _context.Venta.Add(venta);

                    await _context.SaveChangesAsync();
                    
                    if (usuario != null)
                    {
                        var compraExistente = await _context.ProductoComprado
                            .AnyAsync(pc => pc.UserId == usuario.Id && pc.ProductoId == request.Pedido.ProductoId);
                        
                        if (!compraExistente)
                        {
                            var nuevaCompra = new ProductoComprado
                            {
                                UserId = usuario.Id,
                                ProductoId = request.Pedido.ProductoId,
                                FechaCompra = DateTime.UtcNow,
                                VentaId = venta.Id 
                            };
                            _context.ProductoComprado.Add(nuevaCompra);
                            await _context.SaveChangesAsync();
                        }
                    }

                    await transaction.CommitAsync();
                    _logger.LogInformation($"Compra completada: Venta ID {venta.Id}");

                    await Task.Delay(100); 

                    var result = new
                    {
                        VentaId = venta.Id,
                        Estado = "COMPLETADA",
                        Fecha = DateTime.UtcNow,
                        Total = request.Pedido.Total,
                        Mensaje = "Compra procesada exitosamente",
                        NumeroTransaccion = $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}",
                        MetodoPago = $"Tarjeta ****{request.Tarjeta.NumeroTarjeta.Substring(request.Tarjeta.NumeroTarjeta.Length - 4)}",
                        TieneManualPdf = !string.IsNullOrEmpty(producto.ManualUsuarioPdf),
                        MensajeManual = usuario != null && !string.IsNullOrEmpty(producto.ManualUsuarioPdf) 
                            ? "Manual PDF disponible en tu cuenta" 
                            : usuario == null && !string.IsNullOrEmpty(producto.ManualUsuarioPdf)
                            ? "Para acceder al manual PDF, regístrate con el mismo email usado en la compra"
                            : null
                    };
                    
                    return Ok(result);
                }
                catch (Exception innerEx)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(innerEx, "❌ Error en transacción de compra: {Message}", innerEx.Message);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al procesar compra: {Message}", ex.Message);
                
                var errorMessage = ex switch
                {
                    TimeoutException => "La operación tardó demasiado tiempo. Intente nuevamente.",
                    DbUpdateException => "Error al guardar los datos. Intente nuevamente.",
                    InvalidOperationException => "Error en la operación. Verifique los datos ingresados.",
                    _ => "Error interno del servidor al procesar la compra"
                };
                
                return StatusCode(500, new { 
                    mensaje = errorMessage, 
                    codigo = "PURCHASE_ERROR",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("test/{productId}")]
        public async Task<IActionResult> TestProduct(int productId)
        {
            try
            {
                _logger.LogInformation($"=== TEST ENDPOINT - Producto ID: {productId} ===");
                
                var producto = await _context.ProductoHydroLink
                    .FirstOrDefaultAsync(p => p.Id == productId && p.Activo);
                    
                if (producto == null)
                {
                    return NotFound(new { mensaje = "Producto no encontrado", productId = productId });
                }
                
                var result = new
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    TieneManualPdf = !string.IsNullOrEmpty(producto.ManualUsuarioPdf),
                    TamanoManualPdf = string.IsNullOrEmpty(producto.ManualUsuarioPdf) ? 0 : producto.ManualUsuarioPdf.Length,
                    PreviewManual = string.IsNullOrEmpty(producto.ManualUsuarioPdf) ? null : producto.ManualUsuarioPdf.Substring(0, Math.Min(50, producto.ManualUsuarioPdf.Length)),
                    Timestamp = DateTime.UtcNow
                };
                
                _logger.LogInformation($"Producto procesado: {producto.Nombre}, PDF: {result.TieneManualPdf}");
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en test endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }
        
        [HttpPost("validate-card")]
        public IActionResult ValidateCard([FromBody] CardValidationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.NumeroTarjeta))
                {
                    return BadRequest(new { valid = false, mensaje = "Número de tarjeta requerido" });
                }

                var numeroLimpio = request.NumeroTarjeta.Replace(" ", "").Replace("-", "");
                var isValid = numeroLimpio.Length >= 13 && numeroLimpio.Length <= 19 && numeroLimpio.All(char.IsDigit);

                var cardType = GetCardType(numeroLimpio);

                return Ok(new
                {
                    valid = isValid,
                    cardType = cardType,
                    lastFourDigits = isValid ? numeroLimpio.Substring(numeroLimpio.Length - 4) : ""
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar tarjeta");
                return StatusCode(500, new { valid = false, mensaje = "Error al validar tarjeta" });
            }
        }

        private string GetCardType(string numeroTarjeta)
        {
            if (numeroTarjeta.StartsWith("4")) return "Visa";
            if (numeroTarjeta.StartsWith("5") || (numeroTarjeta.StartsWith("2") && numeroTarjeta.Length >= 2 && int.Parse(numeroTarjeta.Substring(0, 2)) >= 22 && int.Parse(numeroTarjeta.Substring(0, 2)) <= 27)) return "Mastercard";
            if (numeroTarjeta.StartsWith("34") || numeroTarjeta.StartsWith("37")) return "American Express";
            if (numeroTarjeta.StartsWith("6")) return "Discover";
            return "Desconocida";
        }
    }

    public class PurchaseProcessRequest
    {
        public ClienteDataDto Cliente { get; set; } = null!;
        public TarjetaDataDto Tarjeta { get; set; } = null!;
        public PedidoDataDto Pedido { get; set; } = null!;
    }

    public class ClienteDataDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
    }

    public class TarjetaDataDto
    {
        public string NumeroTarjeta { get; set; } = string.Empty;
        public string NombreTitular { get; set; } = string.Empty;
        public string MesExpiracion { get; set; } = string.Empty;
        public string AnoExpiracion { get; set; } = string.Empty;
        public string CVV { get; set; } = string.Empty;
    }

    public class PedidoDataDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
        public string? Observaciones { get; set; }
    }

    public class CardValidationRequest
    {
        public string NumeroTarjeta { get; set; } = string.Empty;
    }
}
