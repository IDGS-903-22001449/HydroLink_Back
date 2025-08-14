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
        public async Task<IActionResult> GetProductos([FromQuery] bool includeComponents = false)
        {
            try
            {
                if (includeComponents)
                {
                    var productosConComponentes = await _context.ProductoHydroLink
                        .Where(p => p.Activo)
                        .Take(10) 
                        .OrderByDescending(p => p.Id)
                        .Select(p => new 
                        {
                            id = p.Id,
                            nombre = p.Nombre,
                            descripcion = p.Descripcion,
                            categoria = p.Categoria,
                            precio = p.Precio,
                            activo = p.Activo,
                            fechaCreacion = p.FechaCreacion.ToString("yyyy-MM-dd"),
                            especificaciones = p.Especificaciones ?? "",
                            tipoInstalacion = p.TipoInstalacion ?? "",
                            tiempoInstalacion = p.TiempoInstalacion ?? "",
                            garantia = p.Garantia ?? "",
                            imagenBase64 = (string)null,
                            componentesRequeridos = p.ComponentesRequeridos.Select(cr => new 
                            {
                                id = cr.Id,
                                componenteId = cr.ComponenteId,
                                nombreComponente = cr.Componente.Nombre ?? "",
                                cantidad = cr.Cantidad,
                                unidadMedida = cr.Componente.UnidadMedida ?? "",
                                especificaciones = cr.Especificaciones ?? "",
                                precioUnitario = 150m 
                            }).ToList()
                        })
                        .AsNoTracking()
                        .ToListAsync();

                    return Ok(productosConComponentes);
                }
                else
                {
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
                        })
                        .Take(20) 
                        .OrderByDescending(p => p.Id)
                        .ToListAsync();

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
                .Take(2) 
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
                var producto = await _context.ProductoHydroLink
                    .Include(p => p.ComponentesRequeridos)
                        .ThenInclude(cr => cr.Componente)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (producto == null)
                    return NotFound($"Producto con ID {id} no encontrado");

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
                    ManualUsuarioPdf = producto.ManualUsuarioPdf,
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

            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
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

                if (createDto.CalcularPrecioAutomatico && !createDto.ComponentesRequeridos.Any())
                {
                    return BadRequest("Para calcular el precio automáticamente, debe especificar al menos un componente requerido.");
                }
                
                if (!createDto.CalcularPrecioAutomatico && !createDto.Precio.HasValue)
                {
                    return BadRequest("Debe especificar un precio o habilitar el cálculo automático de precios.");
                }

                var producto = new ProductoHydroLink
                {
                    Nombre = createDto.Nombre,
                    Descripcion = createDto.Descripcion,
                    Categoria = createDto.Categoria,
                    Precio = createDto.Precio ?? 0m, 
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

                var response = new
                {
                    id = producto.Id,
                    mensaje = "Producto creado exitosamente",
                    precioCalculado = createDto.CalcularPrecioAutomatico ? producto.Precio : (decimal?)null,
                    componentesAgregados = createDto.ComponentesRequeridos.Count,
                    tieneManual = !string.IsNullOrEmpty(createDto.ManualUsuarioPdf)
                };

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
                var producto = await _context.ProductoHydroLink
                    .Include(p => p.ComponentesRequeridos)
                    .FirstOrDefaultAsync(p => p.Id == id);
                    
                if (producto == null)
                    return NotFound($"Producto con ID {id} no encontrado");

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

                if (updateDto.CalcularPrecioAutomatico && !updateDto.ComponentesRequeridos.Any())
                {
                    return BadRequest("Para calcular el precio automáticamente, debe especificar al menos un componente requerido.");
                }
                
                if (!updateDto.CalcularPrecioAutomatico && !updateDto.Precio.HasValue)
                {
                    return BadRequest("Debe especificar un precio o habilitar el cálculo automático de precios.");
                }

                producto.Nombre = updateDto.Nombre;
                producto.Descripcion = updateDto.Descripcion;
                producto.Categoria = updateDto.Categoria;
                producto.Precio = updateDto.Precio ?? producto.Precio;
                producto.Especificaciones = updateDto.Especificaciones;
                producto.TipoInstalacion = updateDto.TipoInstalacion;
                producto.TiempoInstalacion = updateDto.TiempoInstalacion;
                producto.Garantia = updateDto.Garantia;
                
                if (!string.IsNullOrWhiteSpace(updateDto.ImagenBase64))
                {
                    producto.ImagenBase64 = updateDto.ImagenBase64;
                }
                
                if (!string.IsNullOrWhiteSpace(updateDto.ManualUsuarioPdf))
                {
                    producto.ManualUsuarioPdf = updateDto.ManualUsuarioPdf;
                }

                _context.ComponenteRequerido.RemoveRange(producto.ComponentesRequeridos);

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

        [HttpGet("{id}/manual-pdf")]
        public async Task<IActionResult> GetProductoPdf(int id)
        {
            try
            {
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
                        UltimaActualizacion = DateTime.UtcNow
                    })
                    .Take(20)
                    .OrderByDescending(p => p.Id)
                    .ToListAsync();

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
                            PreciosCoinciden = diferencia <= 0.50m,
                            producto.ImagenBase64,
                            producto.UltimaActualizacion
                        });
                    }
                    catch (Exception ex)
                    {
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
