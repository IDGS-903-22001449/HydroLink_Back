using HydroLink.Data;
using HydroLink.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HydroLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ManualUsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ManualUsuarioController> _logger;

        public ManualUsuarioController(AppDbContext context, ILogger<ManualUsuarioController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// ULTRA-LIGERO: Solo verifica acceso SIN cargar PDF (para testing)
        /// </summary>
        [HttpGet("producto/{productoId}/manual/test-light")]
        public async Task<IActionResult> TestLigeroSinPdf(int productoId)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("📝 LIGHT TEST: Producto {ProductoId}", productoId);
            
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Solo verificar permisos - SIN cargar PDF
                var permisoInicio = DateTime.UtcNow;
                var tieneAcceso = await _context.ProductoComprado
                    .Where(pc => pc.UserId == userId && pc.ProductoId == productoId)
                    .Join(_context.ProductoHydroLink, pc => pc.ProductoId, p => p.Id, (pc, p) => new {
                        pc.Id,
                        p.Nombre,
                        p.Activo,
                        TienePdf = !string.IsNullOrEmpty(p.ManualUsuarioPdf)
                        // NO seleccionamos p.ManualUsuarioPdf para no cargarlo
                    })
                    .FirstOrDefaultAsync();
                
                var tiempoPermiso = (DateTime.UtcNow - permisoInicio).TotalMilliseconds;
                var tiempoTotal = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                _logger.LogInformation("📝 LIGHT TEST COMPLETADO: {TiempoTotal}ms", tiempoTotal);
                
                if (tieneAcceso == null)
                {
                    return Forbid("No tienes acceso a este producto");
                }
                
                if (!tieneAcceso.Activo)
                {
                    return NotFound("Producto no activo");
                }
                
                return Ok(new
                {
                    success = true,
                    productoId = productoId,
                    nombreProducto = tieneAcceso.Nombre,
                    tieneManual = tieneAcceso.TienePdf,
                    tiempos = new {
                        verificacion_permisos_ms = tiempoPermiso,
                        total_sin_pdf_ms = tiempoTotal
                    },
                    mensaje = "Test rápido completado sin cargar PDF"
                });
            }
            catch (Exception ex)
            {
                var errorTime = DateTime.UtcNow;
                _logger.LogError(ex, "📝 LIGHT TEST ERROR después de {TiempoMs}ms", (errorTime - startTime).TotalMilliseconds);
                return StatusCode(500, "Error interno del servidor");
            }
        }
        
        /// <summary>
        /// STREAMING ULTRA-RÁPIDO: Envía el PDF por chunks para evitar carga masiva
        /// </summary>
        [HttpGet("producto/{productoId}/manual/stream")]
        public async Task<IActionResult> StreamManualProducto(int productoId, [FromQuery] int chunk = 0, [FromQuery] int chunkSize = 100000)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("🌊 STREAM: Chunk {Chunk} del producto {ProductoId}", chunk, productoId);
            
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Solo en el primer chunk, verificar permisos y obtener metadata
                if (chunk == 0)
                {
                    var permisoInicio = DateTime.UtcNow;
                    var haComprado = await _context.ProductoComprado
                        .AnyAsync(pc => pc.UserId == userId && pc.ProductoId == productoId);
                    
                    if (!haComprado)
                    {
                        return Forbid();
                    }
                    
                    // Obtener metadata del PDF
                    var metadata = await _context.ProductoHydroLink
                        .Where(p => p.Id == productoId && p.Activo)
                        .Select(p => new { 
                            p.Id, 
                            p.Nombre, 
                            TamanoPdf = p.ManualUsuarioPdf != null ? p.ManualUsuarioPdf.Length : 0,
                            TienePdf = !string.IsNullOrEmpty(p.ManualUsuarioPdf)
                        })
                        .FirstOrDefaultAsync();
                        
                    var permisoTiempo = (DateTime.UtcNow - permisoInicio).TotalMilliseconds;
                    _logger.LogInformation("STREAM: Permisos verificados en {TiempoMs}ms", permisoTiempo);
                    
                    if (metadata == null || !metadata.TienePdf)
                    {
                        return NotFound("Manual no disponible");
                    }
                    
                    // Retornar información del streaming
                    var totalChunks = (int)Math.Ceiling((double)metadata.TamanoPdf / chunkSize);
                    
                    return Ok(new
                    {
                        productoId = metadata.Id,
                        nombreProducto = metadata.Nombre,
                        streamInfo = new {
                            totalSize = metadata.TamanoPdf,
                            chunkSize = chunkSize,
                            totalChunks = totalChunks,
                            estimatedMB = Math.Round(metadata.TamanoPdf * 0.75 / 1024.0 / 1024.0, 2)
                        },
                        chunk = 0,
                        data = "", // Primer chunk vacío, solo metadata
                        isComplete = false
                    });
                }
                
                // Obtener chunk específico del PDF
                var chunkInicio = DateTime.UtcNow;
                var startIndex = chunk * chunkSize;
                
                // Usar SQL directo para obtener solo el chunk necesario usando SUBSTRING
                var sql = $@"
                    SELECT 
                        p.Id, 
                        p.Nombre,
                        LEN(p.ManualUsuarioPdf) as TotalLength,
                        SUBSTRING(p.ManualUsuarioPdf, {startIndex + 1}, {chunkSize}) as ChunkData
                    FROM ProductoHydroLink p 
                    INNER JOIN ProductoComprado pc ON p.Id = pc.ProductoId 
                    WHERE p.Id = {productoId} 
                    AND pc.UserId = '{userId}' 
                    AND p.Activo = 1";
                
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = sql;
                
                if (_context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
                {
                    await _context.Database.GetDbConnection().OpenAsync();
                }
                
                using var reader = await command.ExecuteReaderAsync();
                
                if (!await reader.ReadAsync())
                {
                    return NotFound();
                }
                
                var nombreProducto = reader["Nombre"].ToString();
                var totalLength = Convert.ToInt32(reader["TotalLength"]);
                var chunkData = reader["ChunkData"].ToString() ?? "";
                
                var chunkTiempo = (DateTime.UtcNow - chunkInicio).TotalMilliseconds;
                var isComplete = startIndex + chunkData.Length >= totalLength;
                
                var finalTime = DateTime.UtcNow;
                _logger.LogInformation("🌊 STREAM CHUNK {Chunk} completado en {TiempoMs}ms - Tamaño: {TamanoChunk}", 
                    chunk, (finalTime - startTime).TotalMilliseconds, chunkData.Length);
                
                return Ok(new
                {
                    productoId = productoId,
                    nombreProducto = nombreProducto,
                    chunk = chunk,
                    data = chunkData,
                    chunkSize = chunkData.Length,
                    totalSize = totalLength,
                    isComplete = isComplete,
                    nextChunk = isComplete ? -1 : chunk + 1,
                    progress = Math.Round((double)(startIndex + chunkData.Length) / totalLength * 100, 1)
                });
            }
            catch (Exception ex)
            {
                var errorTime = DateTime.UtcNow;
                _logger.LogError(ex, "🌊 STREAM ERROR chunk {Chunk} después de {TiempoMs}ms", chunk, (errorTime - startTime).TotalMilliseconds);
                return StatusCode(500, "Error interno del servidor");
            }
        }
        
        /// <summary>
        /// DIAGNÓSTICO: Mide el rendimiento detallado de cada operación
        /// </summary>
        [HttpGet("producto/{productoId}/manual/diagnostico")]
        public async Task<IActionResult> DiagnosticarRendimientoManual(int productoId)
        {
            var inicioTotal = DateTime.UtcNow;
            var diagnostico = new Dictionary<string, object>();
            
            _logger.LogInformation("🔍 DIAGNÓSTICO INICIADO para producto {ProductoId}", productoId);
            
            try
            {
                // 1. AUTH CHECK
                var inicioAuth = DateTime.UtcNow;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
                var tiempoAuth = (DateTime.UtcNow - inicioAuth).TotalMilliseconds;
                diagnostico["auth_ms"] = tiempoAuth;
                
                // 2. VERIFICACIÓN DE COMPRA
                var inicioCompra = DateTime.UtcNow;
                var haComprado = await _context.ProductoComprado
                    .AnyAsync(pc => pc.UserId == userId && pc.ProductoId == productoId);
                var tiempoCompra = (DateTime.UtcNow - inicioCompra).TotalMilliseconds;
                diagnostico["verificacion_compra_ms"] = tiempoCompra;
                
                if (!haComprado)
                {
                    diagnostico["error"] = "Usuario no ha comprado el producto";
                    return Forbid();
                }
                
                // 3. OBTENER SOLO METADATA DEL PRODUCTO
                var inicioMetadata = DateTime.UtcNow;
                var metadata = await _context.ProductoHydroLink
                    .Where(p => p.Id == productoId && p.Activo)
                    .Select(p => new { 
                        p.Id, 
                        p.Nombre, 
                        TamanoPdf = p.ManualUsuarioPdf != null ? p.ManualUsuarioPdf.Length : 0,
                        TienePdf = !string.IsNullOrEmpty(p.ManualUsuarioPdf)
                    })
                    .FirstOrDefaultAsync();
                var tiempoMetadata = (DateTime.UtcNow - inicioMetadata).TotalMilliseconds;
                diagnostico["metadata_query_ms"] = tiempoMetadata;
                
                if (metadata == null)
                {
                    diagnostico["error"] = "Producto no encontrado";
                    return NotFound();
                }
                
                diagnostico["producto_info"] = new {
                    id = metadata.Id,
                    nombre = metadata.Nombre,
                    tiene_pdf = metadata.TienePdf,
                    tamano_pdf_chars = metadata.TamanoPdf,
                    tamano_estimado_mb = metadata.TamanoPdf > 0 ? Math.Round(metadata.TamanoPdf * 0.75 / 1024.0 / 1024.0, 2) : 0
                };
                
                if (!metadata.TienePdf)
                {
                    diagnostico["error"] = "Producto no tiene manual PDF";
                    return NotFound("Manual no disponible");
                }
                
                // 4. OBTENER EL PDF COMPLETO (ESTA ES LA OPERACIÓN COSTOSA)
                var inicioPdf = DateTime.UtcNow;
                var pdfCompleto = await _context.ProductoHydroLink
                    .Where(p => p.Id == productoId)
                    .Select(p => p.ManualUsuarioPdf)
                    .FirstOrDefaultAsync();
                var tiempoPdf = (DateTime.UtcNow - inicioPdf).TotalMilliseconds;
                diagnostico["pdf_query_ms"] = tiempoPdf;
                
                // 5. SERIALIZACIÓN JSON (también puede ser costosa)
                var inicioJson = DateTime.UtcNow;
                var resultado = new
                {
                    productoId = metadata.Id,
                    nombreProducto = metadata.Nombre,
                    manualPdf = pdfCompleto,
                    fechaAcceso = DateTime.UtcNow
                };
                var tiempoJson = (DateTime.UtcNow - inicioJson).TotalMilliseconds;
                diagnostico["json_prep_ms"] = tiempoJson;
                
                var tiempoTotal = (DateTime.UtcNow - inicioTotal).TotalMilliseconds;
                diagnostico["tiempo_total_ms"] = tiempoTotal;
                
                _logger.LogInformation("🔍 DIAGNÓSTICO COMPLETADO: {TiempoTotal}ms - PDF: {TamanoPdf} chars", 
                    tiempoTotal, metadata.TamanoPdf);
                
                // Retornar diagnóstico Y resultado
                return Ok(new
                {
                    diagnostico = diagnostico,
                    resultado = resultado
                });
            }
            catch (Exception ex)
            {
                var tiempoError = (DateTime.UtcNow - inicioTotal).TotalMilliseconds;
                diagnostico["error_ms"] = tiempoError;
                diagnostico["error_message"] = ex.Message;
                
                _logger.LogError(ex, "🔍 DIAGNÓSTICO ERROR después de {TiempoMs}ms", tiempoError);
                return StatusCode(500, new { diagnostico = diagnostico, error = "Error interno del servidor" });
            }
        }
        
        /// <summary>
        /// Obtiene el manual de usuario de un producto ULTRA-RÁPIDO (optimizado para visualización)
        /// </summary>
        [HttpGet("producto/{productoId}/manual/fast")]
        public async Task<IActionResult> ObtenerManualProductoFast(int productoId)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("=== FAST: Obteniendo manual producto {ProductoId} ===", productoId);
            
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // ULTRA-OPTIMIZADO: Una sola consulta sin Include para máximo rendimiento
                var query = from pc in _context.ProductoComprado
                           join p in _context.ProductoHydroLink on pc.ProductoId equals p.Id
                           where pc.UserId == userId && pc.ProductoId == productoId && p.Activo
                           select new {
                               ProductoId = p.Id,
                               ProductoNombre = p.Nombre,
                               ManualPdf = p.ManualUsuarioPdf
                           };

                var resultado = await query.FirstOrDefaultAsync();

                var queryTime = DateTime.UtcNow;
                _logger.LogInformation("FAST query completada en {TiempoMs}ms", (queryTime - startTime).TotalMilliseconds);

                if (resultado == null)
                {
                    return Forbid();
                }

                if (string.IsNullOrEmpty(resultado.ManualPdf))
                {
                    return NotFound("Manual no disponible");
                }

                var finalTime = DateTime.UtcNow;
                _logger.LogInformation("=== FAST COMPLETADO en {TiempoTotalMs}ms ===", (finalTime - startTime).TotalMilliseconds);

                return Ok(new
                {
                    productoId = resultado.ProductoId,
                    nombreProducto = resultado.ProductoNombre,
                    manualPdf = resultado.ManualPdf,
                    fechaAcceso = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                var errorTime = DateTime.UtcNow;
                _logger.LogError(ex, "FAST ERROR después de {TiempoMs}ms", (errorTime - startTime).TotalMilliseconds);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene el manual de usuario de un producto si el usuario lo ha comprado
        /// </summary>
        [HttpGet("producto/{productoId}/manual")]
        public async Task<IActionResult> ObtenerManualProducto(int productoId)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("=== INICIO obtención manual producto {ProductoId} ===", productoId);
            
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Usuario no autenticado");
                }

                _logger.LogInformation("Usuario {UserId} solicitando manual de producto {ProductoId}", userId, productoId);

                // OPTIMIZACIÓN: Verificar compra Y obtener datos del producto en UNA SOLA CONSULTA
                var productoConCompra = await _context.ProductoComprado
                    .Where(pc => pc.UserId == userId && pc.ProductoId == productoId)
                    .Include(pc => pc.Producto)
                    .Select(pc => new {
                        pc.Id,
                        ProductoId = pc.Producto.Id,
                        ProductoNombre = pc.Producto.Nombre,
                        ProductoActivo = pc.Producto.Activo,
                        ManualPdf = pc.Producto.ManualUsuarioPdf
                    })
                    .FirstOrDefaultAsync();

                var consultaTime = DateTime.UtcNow;
                _logger.LogInformation("Consulta BD completada en {TiempoMs}ms", (consultaTime - startTime).TotalMilliseconds);

                if (productoConCompra == null)
                {
                    _logger.LogWarning("Usuario {UserId} no ha comprado producto {ProductoId}", userId, productoId);
                    return Forbid("No tienes acceso al manual de este producto. Debes comprarlo primero.");
                }

                if (!productoConCompra.ProductoActivo)
                {
                    return NotFound("Producto no encontrado o no activo");
                }

                if (string.IsNullOrEmpty(productoConCompra.ManualPdf))
                {
                    _logger.LogInformation("Producto {ProductoId} no tiene manual PDF", productoId);
                    return NotFound("Este producto no tiene manual de usuario disponible");
                }

                var finalTime = DateTime.UtcNow;
                _logger.LogInformation("=== MANUAL OBTENIDO en {TiempoTotalMs}ms - Tamaño: {TamanoPdf} chars ===", 
                    (finalTime - startTime).TotalMilliseconds, 
                    productoConCompra.ManualPdf.Length);

                return Ok(new
                {
                    productoId = productoConCompra.ProductoId,
                    nombreProducto = productoConCompra.ProductoNombre,
                    manualPdf = productoConCompra.ManualPdf,
                    fechaAcceso = DateTime.UtcNow,
                    tiempoRespuesta = (finalTime - startTime).TotalMilliseconds
                });
            }
            catch (Exception ex)
            {
                var errorTime = DateTime.UtcNow;
                _logger.LogError(ex, "ERROR al obtener manual producto {ProductoId} después de {TiempoMs}ms: {Error}", 
                    productoId, (errorTime - startTime).TotalMilliseconds, ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Descarga directamente el PDF del manual de usuario
        /// </summary>
        [HttpGet("producto/{productoId}/manual/download")]
        public async Task<IActionResult> DescargarManualProducto(int productoId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Usuario no autenticado");
                }

                // Verificar si el usuario ha comprado este producto
                var haComprado = await _context.ProductoComprado
                    .AnyAsync(pc => pc.UserId == userId && pc.ProductoId == productoId);

                if (!haComprado)
                {
                    return Forbid("No tienes acceso al manual de este producto. Debes comprarlo primero.");
                }

                // Obtener el producto con su manual
                var producto = await _context.ProductoHydroLink
                    .Where(p => p.Id == productoId && p.Activo)
                    .Select(p => new { p.Id, p.Nombre, p.ManualUsuarioPdf })
                    .FirstOrDefaultAsync();

                if (producto == null)
                {
                    return NotFound("Producto no encontrado");
                }

                if (string.IsNullOrEmpty(producto.ManualUsuarioPdf))
                {
                    return NotFound("Este producto no tiene manual de usuario disponible");
                }

                // Convertir base64 a bytes
                byte[] pdfBytes;
                try
                {
                    pdfBytes = Convert.FromBase64String(producto.ManualUsuarioPdf);
                }
                catch (FormatException)
                {
                    _logger.LogError("El PDF almacenado para el producto {ProductoId} no tiene formato base64 válido", productoId);
                    return StatusCode(500, "Error en el formato del archivo PDF");
                }

                // Generar nombre de archivo seguro
                var fileName = $"Manual_{producto.Nombre.Replace(" ", "_").Replace("/", "_").Replace("\\", "_")}.pdf";
                
                // Retornar el archivo PDF
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar manual de usuario para producto {ProductoId}", productoId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene metadata del manual sin el contenido completo
        /// </summary>
        [HttpGet("producto/{productoId}/manual/info")]
        public async Task<IActionResult> ObtenerInfoManualProducto(int productoId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Usuario no autenticado");
                }

                // Verificar si el usuario ha comprado este producto
                var haComprado = await _context.ProductoComprado
                    .AnyAsync(pc => pc.UserId == userId && pc.ProductoId == productoId);

                if (!haComprado)
                {
                    return Forbid("No tienes acceso al manual de este producto. Debes comprarlo primero.");
                }

                // Obtener información del producto y manual
                var producto = await _context.ProductoHydroLink
                    .Where(p => p.Id == productoId && p.Activo)
                    .Select(p => new { 
                        p.Id, 
                        p.Nombre, 
                        TieneManual = !string.IsNullOrEmpty(p.ManualUsuarioPdf),
                        TamanoManual = !string.IsNullOrEmpty(p.ManualUsuarioPdf) ? p.ManualUsuarioPdf.Length : 0
                    })
                    .FirstOrDefaultAsync();

                if (producto == null)
                {
                    return NotFound("Producto no encontrado");
                }

                return Ok(new
                {
                    productoId = producto.Id,
                    nombreProducto = producto.Nombre,
                    tieneManual = producto.TieneManual,
                    tamanoEstimado = producto.TamanoManual > 0 ? (producto.TamanoManual * 3 / 4) : 0, // Aproximación del tamaño real
                    fechaConsulta = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener info del manual para producto {ProductoId}", productoId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene todos los productos comprados por el usuario actual con sus manuales
        /// </summary>
        [HttpGet("mis-productos")]
        public async Task<IActionResult> ObtenerMisProductos()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Usuario no autenticado");
                }

                var productosComprados = await _context.ProductoComprado
                    .Where(pc => pc.UserId == userId)
                    .Include(pc => pc.Producto)
                    .OrderByDescending(pc => pc.FechaCompra)
                    .Select(pc => new
                    {
                        id = pc.Id,
                        fechaCompra = pc.FechaCompra,
                        producto = new
                        {
                            id = pc.Producto.Id,
                            nombre = pc.Producto.Nombre,
                            descripcion = pc.Producto.Descripcion,
                            categoria = pc.Producto.Categoria,
                            precio = pc.Producto.Precio,
                            imagenBase64 = pc.Producto.ImagenBase64,
                            tieneManual = !string.IsNullOrEmpty(pc.Producto.ManualUsuarioPdf),
                            especificaciones = pc.Producto.Especificaciones,
                            tipoInstalacion = pc.Producto.TipoInstalacion,
                            tiempoInstalacion = pc.Producto.TiempoInstalacion,
                            garantia = pc.Producto.Garantia
                        }
                    })
                    .ToListAsync();

                return Ok(productosComprados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos comprados por usuario {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Registra la compra de un producto por parte de un usuario
        /// (Este método sería llamado desde el proceso de venta/cotización)
        /// </summary>
        [HttpPost("registrar-compra")]
        public async Task<IActionResult> RegistrarCompraProducto([FromBody] RegistrarCompraDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Usuario no autenticado");
                }

                // Verificar que el producto existe
                var producto = await _context.ProductoHydroLink.FindAsync(dto.ProductoId);
                if (producto == null)
                {
                    return NotFound("Producto no encontrado");
                }

                // Verificar si ya existe una compra registrada para evitar duplicados
                var compraExistente = await _context.ProductoComprado
                    .FirstOrDefaultAsync(pc => pc.UserId == userId && pc.ProductoId == dto.ProductoId);

                if (compraExistente != null)
                {
                    return Ok(new { mensaje = "La compra ya estaba registrada", compraId = compraExistente.Id });
                }

                // Crear nuevo registro de compra
                var nuevaCompra = new ProductoComprado
                {
                    UserId = userId,
                    ProductoId = dto.ProductoId,
                    FechaCompra = DateTime.UtcNow,
                    VentaId = dto.VentaId
                };

                _context.ProductoComprado.Add(nuevaCompra);
                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    mensaje = "Compra registrada exitosamente",
                    compraId = nuevaCompra.Id,
                    tieneManual = !string.IsNullOrEmpty(producto.ManualUsuarioPdf)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar compra de producto {ProductoId}", dto?.ProductoId);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }

    public class RegistrarCompraDto
    {
        public int ProductoId { get; set; }
        public int? VentaId { get; set; }
    }
}
