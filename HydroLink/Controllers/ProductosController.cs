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
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICostoPromedioService _costoPromedioService;
        private readonly IPrecioComponenteService _precioService;
        private readonly IProductoPrecioService _productoPrecioService;

        public ProductosController(
            AppDbContext context, 
            ICostoPromedioService costoPromedioService,
            IPrecioComponenteService precioService,
            IProductoPrecioService productoPrecioService)
        {
            _context = context;
            _costoPromedioService = costoPromedioService;
            _precioService = precioService;
            _productoPrecioService = productoPrecioService;
        }

        // GET: api/productos
        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            try
            {
                // ULTRA-OPTIMIZACIÓN: Solo campos esenciales para la lista de productos
                var productos = await _context.ProductoHydroLink
                    .Where(p => p.Activo)
                    .Select(p => new 
                    {
                        p.Id,
                        p.Nombre,
                        p.Descripcion,
                        p.Categoria,
                        p.Precio,
                        p.ImagenBase64
                        // Eliminar campos no esenciales para mejorar rendimiento
                    })
                    .Take(20) // Limitar resultados
                    .OrderByDescending(p => p.Id) // Ordenar por ID es más rápido
                    .ToListAsync();

                // Mapear directamente a un objeto simple para máximo rendimiento
                var productosDto = productos.Select(p => new 
                {
                    id = p.Id,
                    nombre = p.Nombre,
                    descripcion = p.Descripcion,
                    categoria = p.Categoria,
                    precio = p.Precio,
                    imagenBase64 = p.ImagenBase64
                }).ToList();

                return Ok(productosDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    mensaje = "Error al obtener productos", 
                    detalle = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // GET: api/productos/home
        [HttpGet("home")]
        public async Task<IActionResult> GetProductosParaHome()
        {
            var productos = await _context.ProductoHydroLink
                .Where(p => p.Activo)
                .OrderByDescending(p => p.FechaCreacion)
                .Take(2) // Limitar a solo 2 productos
                .Select(p => new ProductoHomeDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Categoria = p.Categoria,
                    Precio = p.Precio,
                    ImagenBase64 = p.ImagenBase64
                })
                .ToListAsync();

            return Ok(productos);
        }

        // GET: api/productos/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProducto(int id, [FromQuery] bool includePdf = false)
        {
            if (includePdf)
            {
                // VERSIÓN CON PDF: Carga completa del producto incluyendo el PDF
                var producto = await _context.ProductoHydroLink
                    .Include(p => p.ComponentesRequeridos)
                        .ThenInclude(cr => cr.Componente)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (producto == null)
                    return NotFound($"Producto con ID {id} no encontrado");

                // Calcular precio estimado basado en componentes
                decimal precioEstimadoComponentes = 0;
                foreach (var cr in producto.ComponentesRequeridos)
                {
                    var precioComponente = await _precioService.ObtenerPrecioPromedioAsync(cr.ComponenteId);
                    precioEstimadoComponentes += precioComponente * cr.Cantidad;
                }

                var productoDto = new ProductoHydroLinkDto
                {
                    Id = producto.Id,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Categoria = producto.Categoria,
                    Precio = producto.Precio,
                    PrecioEstimadoComponentes = precioEstimadoComponentes,
                    Activo = producto.Activo,
                    FechaCreacion = producto.FechaCreacion,
                    Especificaciones = producto.Especificaciones,
                    TipoInstalacion = producto.TipoInstalacion,
                    TiempoInstalacion = producto.TiempoInstalacion,
                    Garantia = producto.Garantia,
                    ImagenBase64 = producto.ImagenBase64,
                    TieneManual = !string.IsNullOrEmpty(producto.ManualUsuarioPdf),
                    ManualUsuarioPdf = producto.ManualUsuarioPdf, // Incluir PDF completo
                    ComponentesRequeridos = producto.ComponentesRequeridos.Select(cr => new ComponenteRequeridoDto
                    {
                        Id = cr.Id,
                        ComponenteId = cr.ComponenteId,
                        NombreComponente = cr.Componente?.Nombre ?? "",
                        Cantidad = cr.Cantidad,
                        UnidadMedida = cr.Componente?.UnidadMedida ?? "",
                        Especificaciones = cr.Especificaciones
                    }).ToList()
                };

                return Ok(productoDto);
            }
            else
            {
                // VERSIÓN OPTIMIZADA: Sin cargar el PDF para mejor rendimiento
                var producto = await _context.ProductoHydroLink
                    .Where(p => p.Id == id)
                    .Select(p => new
                    {
                        p.Id,
                        p.Nombre,
                        p.Descripcion,
                        p.Categoria,
                        p.Precio,
                        p.Activo,
                        p.FechaCreacion,
                        p.Especificaciones,
                        p.TipoInstalacion,
                        p.TiempoInstalacion,
                        p.Garantia,
                        p.ImagenBase64,
                        TieneManual = !string.IsNullOrEmpty(p.ManualUsuarioPdf),
                        ComponentesRequeridos = p.ComponentesRequeridos.Select(cr => new
                        {
                            cr.Id,
                            cr.ComponenteId,
                            cr.Cantidad,
                            cr.Especificaciones,
                            NombreComponente = cr.Componente.Nombre,
                            UnidadMedida = cr.Componente.UnidadMedida
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (producto == null)
                    return NotFound($"Producto con ID {id} no encontrado");

                // Calcular precio estimado basado en componentes
                decimal precioEstimadoComponentes = 0;
                foreach (var cr in producto.ComponentesRequeridos)
                {
                    var precioComponente = await _precioService.ObtenerPrecioPromedioAsync(cr.ComponenteId);
                    precioEstimadoComponentes += precioComponente * cr.Cantidad;
                }

                var productoDto = new ProductoHydroLinkDto
                {
                    Id = producto.Id,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Categoria = producto.Categoria,
                    Precio = producto.Precio,
                    PrecioEstimadoComponentes = precioEstimadoComponentes,
                    Activo = producto.Activo,
                    FechaCreacion = producto.FechaCreacion,
                    Especificaciones = producto.Especificaciones,
                    TipoInstalacion = producto.TipoInstalacion,
                    TiempoInstalacion = producto.TiempoInstalacion,
                    Garantia = producto.Garantia,
                    ImagenBase64 = producto.ImagenBase64,
                    TieneManual = producto.TieneManual,
                    // ManualUsuarioPdf se omite intencionalmente para optimizar rendimiento
                    ComponentesRequeridos = producto.ComponentesRequeridos.Select(cr => new ComponenteRequeridoDto
                    {
                        Id = cr.Id,
                        ComponenteId = cr.ComponenteId,
                        NombreComponente = cr.NombreComponente,
                        Cantidad = cr.Cantidad,
                        UnidadMedida = cr.UnidadMedida,
                        Especificaciones = cr.Especificaciones
                    }).ToList()
                };

                return Ok(productoDto);
            }
        }

        // POST: api/productos
        [HttpPost]
        public async Task<IActionResult> CrearProducto([FromBody] ProductoHydroLinkCreateDto createDto)
        {
            if (createDto == null)
                return BadRequest("Datos del producto inválidos");

            // Comentamos temporalmente la validación de imagen requerida
            // if (string.IsNullOrWhiteSpace(createDto.ImagenBase64))
            //     return BadRequest("La imagen del producto es requerida en formato base64.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Validar que todos los componentes existan
                var componenteIds = createDto.ComponentesRequeridos.Select(c => c.ComponenteId).ToList();
                if (componenteIds.Any())
                {
                    var componentesExistentes = await _context.Componente
                        .Where(c => componenteIds.Contains(c.Id) && c.Activo)
                        .Select(c => c.Id)
                        .ToListAsync();
                    
                    var componentesInexistentes = componenteIds.Except(componentesExistentes).ToList();
                    if (componentesInexistentes.Any())
                    {
                        return BadRequest($"Los siguientes componentes no existen o están inactivos: {string.Join(", ", componentesInexistentes)}");
                    }
                }

                // Validar lógica de precio
                if (createDto.CalcularPrecioAutomatico && !createDto.ComponentesRequeridos.Any())
                {
                    return BadRequest("Para calcular el precio automáticamente, debe especificar al menos un componente requerido.");
                }
                
                if (!createDto.CalcularPrecioAutomatico && !createDto.Precio.HasValue)
                {
                    return BadRequest("Debe especificar un precio o habilitar el cálculo automático de precios.");
                }

                // Crear el producto
                var producto = new ProductoHydroLink
                {
                    Nombre = createDto.Nombre,
                    Descripcion = createDto.Descripcion,
                    Categoria = createDto.Categoria,
                    Precio = createDto.Precio ?? 0m, // Se calculará después si es automático
                    Especificaciones = createDto.Especificaciones,
                    TipoInstalacion = createDto.TipoInstalacion,
                    TiempoInstalacion = createDto.TiempoInstalacion,
                    Garantia = createDto.Garantia,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow,
                    ImagenBase64 = createDto.ImagenBase64,
                    ManualUsuarioPdf = createDto.ManualUsuarioPdf
                };

                _context.ProductoHydroLink.Add(producto);
                await _context.SaveChangesAsync();

                // Agregar los componentes requeridos
                foreach (var componenteDto in createDto.ComponentesRequeridos)
                {
                    var componenteRequerido = new ComponenteRequerido
                    {
                        ProductoHydroLinkId = producto.Id,
                        ComponenteId = componenteDto.ComponenteId,
                        Cantidad = componenteDto.Cantidad,
                        Especificaciones = componenteDto.Especificaciones
                    };
                    
                    _context.ComponenteRequerido.Add(componenteRequerido);
                }
                
                await _context.SaveChangesAsync();

                // Si se solicita calcular precio automáticamente
                if (createDto.CalcularPrecioAutomatico && createDto.ComponentesRequeridos.Any())
                {
                    var nuevoPrecio = await _costoPromedioService.CalcularPrecioProductoHydroLinkAsync(producto.Id, createDto.MargenGanancia);
                    if (nuevoPrecio > 0)
                    {
                        producto.Precio = nuevoPrecio;
                        await _context.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();

                // OPTIMIZADO: No cargar el producto completo, solo devolver datos básicos
                var response = new
                {
                    id = producto.Id,
                    mensaje = "Producto creado exitosamente",
                    precioCalculado = createDto.CalcularPrecioAutomatico ? producto.Precio : (decimal?)null,
                    componentesAgregados = createDto.ComponentesRequeridos.Count,
                    tieneManual = !string.IsNullOrEmpty(createDto.ManualUsuarioPdf)
                };

                // Devolver referencia al producto sin incluir PDF en la respuesta
                return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new 
                { 
                    mensaje = "Error al crear el producto", 
                    detalle = ex.Message 
                });
            }
        }

        // POST: api/productos/{id}/componentes
        [HttpPost("{id}/componentes")]
        public async Task<IActionResult> AgregarComponenteRequerido(int id, [FromBody] ComponenteRequeridoCreateDto componenteDto)
        {
            var producto = await _context.ProductoHydroLink.FindAsync(id);
            if (producto == null)
                return NotFound($"Producto con ID {id} no encontrado");

            var componente = await _context.Componente.FindAsync(componenteDto.ComponenteId);
            if (componente == null)
                return NotFound($"Componente con ID {componenteDto.ComponenteId} no encontrado");

            var componenteRequerido = new ComponenteRequerido
            {
                ProductoHydroLinkId = id,
                ComponenteId = componenteDto.ComponenteId,
                Cantidad = componenteDto.Cantidad,
                Especificaciones = componenteDto.Especificaciones
            };

            _context.ComponenteRequerido.Add(componenteRequerido);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Componente agregado al producto exitosamente", componenteId = componenteRequerido.Id });
        }

        // PUT: api/productos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarProducto(int id, [FromBody] ProductoHydroLinkCreateDto updateDto)
        {
            if (updateDto == null)
                return BadRequest("Datos del producto inválidos");

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Buscar el producto existente
                var producto = await _context.ProductoHydroLink
                    .Include(p => p.ComponentesRequeridos)
                    .FirstOrDefaultAsync(p => p.Id == id);
                    
                if (producto == null)
                    return NotFound($"Producto con ID {id} no encontrado");

                // Validar que todos los componentes existan si se proporcionan
                var componenteIds = updateDto.ComponentesRequeridos.Select(c => c.ComponenteId).ToList();
                if (componenteIds.Any())
                {
                    var componentesExistentes = await _context.Componente
                        .Where(c => componenteIds.Contains(c.Id) && c.Activo)
                        .Select(c => c.Id)
                        .ToListAsync();
                    
                    var componentesInexistentes = componenteIds.Except(componentesExistentes).ToList();
                    if (componentesInexistentes.Any())
                    {
                        return BadRequest($"Los siguientes componentes no existen o están inactivos: {string.Join(", ", componentesInexistentes)}");
                    }
                }

                // Validar lógica de precio
                if (updateDto.CalcularPrecioAutomatico && !updateDto.ComponentesRequeridos.Any())
                {
                    return BadRequest("Para calcular el precio automáticamente, debe especificar al menos un componente requerido.");
                }
                
                if (!updateDto.CalcularPrecioAutomatico && !updateDto.Precio.HasValue)
                {
                    return BadRequest("Debe especificar un precio o habilitar el cálculo automático de precios.");
                }

                // Actualizar datos del producto
                producto.Nombre = updateDto.Nombre;
                producto.Descripcion = updateDto.Descripcion;
                producto.Categoria = updateDto.Categoria;
                producto.Precio = updateDto.Precio ?? producto.Precio;
                producto.Especificaciones = updateDto.Especificaciones;
                producto.TipoInstalacion = updateDto.TipoInstalacion;
                producto.TiempoInstalacion = updateDto.TiempoInstalacion;
                producto.Garantia = updateDto.Garantia;
                
                // Actualizar imagen solo si se proporciona una nueva
                if (!string.IsNullOrWhiteSpace(updateDto.ImagenBase64))
                {
                    producto.ImagenBase64 = updateDto.ImagenBase64;
                }
                
                // Actualizar manual solo si se proporciona uno nuevo
                if (!string.IsNullOrWhiteSpace(updateDto.ManualUsuarioPdf))
                {
                    producto.ManualUsuarioPdf = updateDto.ManualUsuarioPdf;
                }

                // Eliminar componentes existentes
                _context.ComponenteRequerido.RemoveRange(producto.ComponentesRequeridos);

                // Agregar los nuevos componentes requeridos
                foreach (var componenteDto in updateDto.ComponentesRequeridos)
                {
                    var componenteRequerido = new ComponenteRequerido
                    {
                        ProductoHydroLinkId = producto.Id,
                        ComponenteId = componenteDto.ComponenteId,
                        Cantidad = componenteDto.Cantidad,
                        Especificaciones = componenteDto.Especificaciones
                    };
                    
                    _context.ComponenteRequerido.Add(componenteRequerido);
                }
                
                await _context.SaveChangesAsync();

                // Si se solicita calcular precio automáticamente
                if (updateDto.CalcularPrecioAutomatico && updateDto.ComponentesRequeridos.Any())
                {
                    var nuevoPrecio = await _costoPromedioService.CalcularPrecioProductoHydroLinkAsync(producto.Id, updateDto.MargenGanancia);
                    if (nuevoPrecio > 0)
                    {
                        producto.Precio = nuevoPrecio;
                        await _context.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();

                return Ok(new { 
                    mensaje = "Producto actualizado exitosamente",
                    precioCalculado = updateDto.CalcularPrecioAutomatico ? producto.Precio : (decimal?)null,
                    componentesActualizados = updateDto.ComponentesRequeridos.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new 
                { 
                    mensaje = "Error al actualizar el producto", 
                    detalle = ex.Message 
                });
            }
        }

        // DELETE: api/productos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var producto = await _context.ProductoHydroLink.FindAsync(id);
            if (producto == null)
                return NotFound($"Producto con ID {id} no encontrado");

            // Eliminación lógica
            producto.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Producto desactivado exitosamente" });
        }

        // POST: api/productos/{id}/calcular-precio
        [HttpPost("{id}/calcular-precio")]
        public async Task<IActionResult> CalcularYActualizarPrecio(int id, [FromQuery] decimal margenGanancia = 0.30m)
        {
            var producto = await _context.ProductoHydroLink.FindAsync(id);
            if (producto == null)
                return NotFound($"Producto con ID {id} no encontrado");

            var nuevoPrecio = await _costoPromedioService.CalcularPrecioProductoHydroLinkAsync(id, margenGanancia);
            
            if (nuevoPrecio <= 0)
            {
                return BadRequest("No se pudo calcular el precio. Verifique que los componentes y sus costos estén configurados.");
            }
            
            producto.Precio = nuevoPrecio;
            await _context.SaveChangesAsync();

            return Ok(new { 
                mensaje = "Precio calculado y actualizado exitosamente", 
                nuevoPrecio = producto.Precio 
            });
        }

        // GET: api/productos/{id}/preview-costo
        [HttpGet("{id}/preview-costo")]
        public async Task<IActionResult> PreviewCostoProducto(int id)
        {
            var producto = await _context.ProductoHydroLink
                .Include(p => p.ComponentesRequeridos)
                    .ThenInclude(cr => cr.Componente)
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

            if (producto == null)
                return NotFound($"Producto con ID {id} no encontrado");

            var componentesCalculados = new List<object>();
            decimal totalEstimadoComponentes = 0;
            
            foreach (var cr in producto.ComponentesRequeridos)
            {
                var precioUnitario = await _precioService.ObtenerPrecioPromedioAsync(cr.ComponenteId);
                var subtotal = cr.Cantidad * precioUnitario;
                
                componentesCalculados.Add(new
                {
                    nombre = cr.Componente?.Nombre,
                    categoria = cr.Componente?.Categoria,
                    cantidad = cr.Cantidad,
                    unidadMedida = cr.Componente?.UnidadMedida,
                    precioUnitario = precioUnitario,
                    subtotal = subtotal,
                    especificaciones = cr.Especificaciones
                });
                
                totalEstimadoComponentes += subtotal;
            }

            var preview = new
            {
                producto = new
                {
                    id = producto.Id,
                    nombre = producto.Nombre,
                    descripcion = producto.Descripcion,
                    precioVenta = producto.Precio
                },
                componentesCalculados = componentesCalculados,
                totalEstimadoComponentes = totalEstimadoComponentes,
                margenGanancia = producto.Precio - totalEstimadoComponentes,
                porcentajeMargen = totalEstimadoComponentes > 0 ? ((producto.Precio - totalEstimadoComponentes) / totalEstimadoComponentes) * 100 : 0
            };

            return Ok(preview);
        }

        /// <summary>
        /// Endpoint optimizado específicamente para obtener el PDF de un producto
        /// </summary>
        [HttpGet("{id}/manual-pdf")]
        public async Task<IActionResult> GetProductoPdf(int id)
        {
            try
            {
                // ULTRA-OPTIMIZADO: Solo cargar el PDF específico sin otros datos
                var pdfData = await _context.ProductoHydroLink
                    .Where(p => p.Id == id && p.Activo)
                    .Select(p => new
                    {
                        p.Id,
                        p.Nombre,
                        p.ManualUsuarioPdf,
                        TieneManual = !string.IsNullOrEmpty(p.ManualUsuarioPdf)
                    })
                    .FirstOrDefaultAsync();

                if (pdfData == null)
                    return NotFound($"Producto con ID {id} no encontrado");

                if (!pdfData.TieneManual)
                    return NotFound("Este producto no tiene manual de usuario disponible");

                return Ok(new
                {
                    productoId = pdfData.Id,
                    nombreProducto = pdfData.Nombre,
                    manualPdf = pdfData.ManualUsuarioPdf,
                    fechaAcceso = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener el PDF del producto",
                    detalle = ex.Message
                });
            }
        }

        // GET: api/productos/precios-actualizados
        /// <summary>
        /// Endpoint para verificar que los precios estén actualizados con los últimos costos de materias primas
        /// </summary>
        [HttpGet("precios-actualizados")]
        public async Task<IActionResult> GetProductosConPreciosActualizados()
        {
            try
            {
                var productos = await _context.ProductoHydroLink
                    .Where(p => p.Activo)
                    .Select(p => new 
                    {
                        p.Id,
                        p.Nombre,
                        p.Descripcion,
                        p.Categoria,
                        PrecioActual = p.Precio,
                        p.ImagenBase64,
                        // Incluir timestamp para verificar frecuencia de actualización
                        UltimaActualizacion = DateTime.UtcNow
                    })
                    .Take(20)
                    .OrderByDescending(p => p.Id)
                    .ToListAsync();

                // Calcular precio estimado dinámicamente para comparación
                var productosConComparacion = new List<object>();
                foreach (var producto in productos)
                {
                    try
                    {
                        var precioEstimado = await _costoPromedioService.CalcularPrecioProductoHydroLinkAsync(producto.Id, 0.30m);
                        var diferencia = Math.Abs(producto.PrecioActual - precioEstimado);
                        var porcentajeDiferencia = producto.PrecioActual > 0 ? (diferencia / producto.PrecioActual) * 100 : 0;
                        
                        productosConComparacion.Add(new
                        {
                            producto.Id,
                            producto.Nombre,
                            producto.Descripcion,
                            producto.Categoria,
                            PrecioAlmacenado = producto.PrecioActual,
                            PrecioCalculadoDinamicamente = precioEstimado,
                            DiferenciaDolares = diferencia,
                            DiferenciaPorcentaje = porcentajeDiferencia,
                            PreciosCoinciden = diferencia <= 0.50m, // Tolerancia de 50 centavos
                            producto.ImagenBase64,
                            producto.UltimaActualizacion
                        });
                    }
                    catch (Exception ex)
                    {
                        // En caso de error al calcular precio dinámico, usar solo el precio almacenado
                        productosConComparacion.Add(new
                        {
                            producto.Id,
                            producto.Nombre,
                            producto.Descripcion,
                            producto.Categoria,
                            PrecioAlmacenado = producto.PrecioActual,
                            PrecioCalculadoDinamicamente = (decimal?)null,
                            ErrorCalculoDinamico = ex.Message,
                            producto.ImagenBase64,
                            producto.UltimaActualizacion
                        });
                    }
                }

                return Ok(new
                {
                    productos = productosConComparacion,
                    timestamp = DateTime.UtcNow,
                    mensaje = "Productos con verificación de precios actualizados"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    mensaje = "Error al obtener productos con precios actualizados", 
                    detalle = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // POST: api/productos/forzar-actualizacion-precios
        /// <summary>
        /// Endpoint para forzar la actualización de precios de todos los productos
        /// </summary>
        [HttpPost("forzar-actualizacion-precios")]
        public async Task<IActionResult> ForzarActualizacionPrecios([FromQuery] decimal margenGanancia = 0.30m)
        {
            try
            {
                var productos = await _context.ProductoHydroLink
                    .Where(p => p.Activo)
                    .Select(p => new { p.Id, p.Nombre, PrecioAnterior = p.Precio })
                    .ToListAsync();

                var productosActualizados = new List<object>();
                var errores = new List<string>();

                foreach (var producto in productos)
                {
                    try
                    {
                        var nuevoPrecio = await _costoPromedioService.CalcularPrecioProductoHydroLinkAsync(producto.Id, margenGanancia);
                        
                        // Actualizar en base de datos
                        var productoEntity = await _context.ProductoHydroLink.FindAsync(producto.Id);
                        if (productoEntity != null)
                        {
                            productoEntity.Precio = nuevoPrecio;
                            await _context.SaveChangesAsync();

                            productosActualizados.Add(new
                            {
                                id = producto.Id,
                                nombre = producto.Nombre,
                                precioAnterior = producto.PrecioAnterior,
                                precioNuevo = nuevoPrecio,
                                diferencia = nuevoPrecio - producto.PrecioAnterior,
                                actualizado = true
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Error actualizando producto {producto.Id} ({producto.Nombre}): {ex.Message}");
                    }
                }

                return Ok(new
                {
                    mensaje = "Actualización de precios completada",
                    productosActualizados = productosActualizados.Count,
                    totalProductos = productos.Count,
                    margenUtilizado = margenGanancia,
                    productos = productosActualizados,
                    errores = errores,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    mensaje = "Error al forzar actualización de precios", 
                    detalle = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
