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

        /// <summary>
        /// Procesar una compra con información de pago (simulado)
        /// </summary>
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPurchase([FromBody] PurchaseProcessRequest request)
        {
            _logger.LogInformation("=== INICIANDO PROCESO DE COMPRA OPTIMIZADO ===");
            
            try
            {
                // Validar datos básicos
                if (request == null || request.Pedido == null)
                {
                    _logger.LogError("Datos de compra inválidos: request o Pedido es null");
                    return BadRequest(new { mensaje = "Datos de compra inválidos" });
                }
                
                // Validaciones rápidas antes de interacción con BD
                if (string.IsNullOrEmpty(request.Tarjeta?.NumeroTarjeta) || request.Tarjeta.NumeroTarjeta.Length < 13)
                {
                    return BadRequest(new { mensaje = "Número de tarjeta inválido" });
                }
                
                if (string.IsNullOrEmpty(request.Cliente?.Email) || !request.Cliente.Email.Contains("@"))
                {
                    return BadRequest(new { mensaje = "Email del cliente inválido" });
                }

                _logger.LogInformation($"Procesando compra para producto ID: {request.Pedido.ProductoId}, Email: {request.Cliente.Email}");

                // Transacción con timeout optimizado
                _context.Database.SetCommandTimeout(30); // 30 segundos máximo
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // OPTIMIZACIÓN: Cargar producto y usuario en paralelo
                    var productoTask = _context.ProductoHydroLink
                        .FirstOrDefaultAsync(p => p.Id == request.Pedido.ProductoId && p.Activo);
                    
                    var usuarioTask = _context.Users
                        .FirstOrDefaultAsync(u => u.Email == request.Cliente.Email);
                    
                    var clienteTask = _context.Persona.OfType<Cliente>()
                        .FirstOrDefaultAsync(c => c.Email == request.Cliente.Email);

                    // Esperar todas las consultas en paralelo
                    await Task.WhenAll(productoTask, usuarioTask, clienteTask);
                    
                    var producto = productoTask.Result;
                    var usuario = usuarioTask.Result;
                    var cliente = clienteTask.Result;
                    
                    if (producto == null)
                    {
                        _logger.LogError($"Producto no encontrado con ID: {request.Pedido.ProductoId}");
                        return BadRequest(new { mensaje = "Producto no encontrado o no disponible" });
                    }
                    
                    _logger.LogInformation($"Producto: {producto.Nombre}, PDF: {!string.IsNullOrEmpty(producto.ManualUsuarioPdf)}");

                    // Crear cliente si no existe
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
                        await _context.SaveChangesAsync(); // Necesario para obtener el ID
                        _logger.LogInformation($"Cliente creado con ID: {cliente.Id}");
                    }
                    else
                    {
                        _logger.LogInformation($"Cliente existente encontrado con ID: {cliente.Id}");
                    }

                    // Crear la venta
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

                    // Registrar compra para PDF si el usuario existe
                    if (usuario != null)
                    {
                        // Verificar si ya existe la compra
                        var compraExistente = await _context.ProductoComprado
                            .AnyAsync(pc => pc.UserId == usuario.Id && pc.ProductoId == request.Pedido.ProductoId);
                        
                        if (!compraExistente)
                        {
                            var nuevaCompra = new ProductoComprado
                            {
                                UserId = usuario.Id,
                                ProductoId = request.Pedido.ProductoId,
                                FechaCompra = DateTime.UtcNow,
                                VentaId = 0 // Se actualizará después de SaveChanges
                            };
                            _context.ProductoComprado.Add(nuevaCompra);
                        }
                    }

                    // Guardar todos los cambios de una vez
                    await _context.SaveChangesAsync();
                    
                    // Actualizar VentaId si se creó ProductoComprado
                    if (usuario != null)
                    {
                        var productoCompradoSinVentaId = await _context.ProductoComprado
                            .FirstOrDefaultAsync(pc => pc.UserId == usuario.Id && 
                                                      pc.ProductoId == request.Pedido.ProductoId && 
                                                      pc.VentaId == 0);
                        if (productoCompradoSinVentaId != null)
                        {
                            productoCompradoSinVentaId.VentaId = venta.Id;
                            await _context.SaveChangesAsync();
                        }
                    }

                    await transaction.CommitAsync();
                    _logger.LogInformation($"✅ Compra completada: Venta ID {venta.Id}");

                    // SIMULACIÓN RÁPIDA de procesamiento de pago (solo para testing)
                    await Task.Delay(100); // Reducido de 1000ms a 100ms

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
                
                // Manejo detallado de errores
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

        /// <summary>
        /// Endpoint de prueba para diagnosticar problemas
        /// </summary>
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
        
        /// <summary>
        /// Validar datos de tarjeta (simulado)
        /// </summary>
        [HttpPost("validate-card")]
        public IActionResult ValidateCard([FromBody] CardValidationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.NumeroTarjeta))
                {
                    return BadRequest(new { valid = false, mensaje = "Número de tarjeta requerido" });
                }

                // Validación básica de longitud
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

    // DTOs para el controlador
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
