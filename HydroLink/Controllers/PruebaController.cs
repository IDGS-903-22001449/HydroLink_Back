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

namespace HydroLink.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PruebaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PruebaController> _logger;
        private readonly IPrecioActualizacionService _precioActualizacionService;
        private readonly ICostoPromedioService _costoPromedioService;

        public PruebaController(
            AppDbContext context,
            ILogger<PruebaController> logger,
            IPrecioActualizacionService precioActualizacionService,
            ICostoPromedioService costoPromedioService)
        {
            _context = context;
            _logger = logger;
            _precioActualizacionService = precioActualizacionService;
            _costoPromedioService = costoPromedioService;
        }

        // Endpoint para probar compra sin autenticación
        [HttpPost("test-purchase")]
        public async Task<ActionResult<object>> TestPurchase()
        {
            try
            {
                // Obtener primer proveedor disponible
                var proveedor = await _context.Proveedor.FirstOrDefaultAsync();
                if (proveedor == null)
                {
                    return BadRequest("No hay proveedores disponibles");
                }

                // Obtener materia prima Arduino Uno
                var materiaPrima = await _context.MateriaPrima
                    .FirstOrDefaultAsync(mp => mp.Name.Contains("Arduino Uno"));
                if (materiaPrima == null)
                {
                    return BadRequest("No se encontró la materia prima Arduino Uno");
                }

                _logger.LogInformation("Estado antes de la compra - Arduino Uno: Stock={Stock}, Costo={CostoUnitario}", 
                    materiaPrima.Stock, materiaPrima.CostoUnitario);

                // Crear compra
                var compra = new Compra
                {
                    Fecha = DateTime.UtcNow,
                    ProveedorId = proveedor.Id,
                    Detalles = new List<CompraDetalle>()
                };
                _context.Compra.Add(compra);
                await _context.SaveChangesAsync();

                // Agregar detalle con precio más alto
                var detalle = new CompraDetalle
                {
                    CompraId = compra.Id,
                    MateriaPrimaId = materiaPrima.Id,
                    Cantidad = 5, // Comprar 5 unidades
                    PrecioUnitario = 1200.00m // Precio MUCHO más alto para probar actualización
                };
                compra.Detalles.Add(detalle);

                // Actualizar stock y calcular costo promedio
                var stockAnterior = materiaPrima.Stock;
                var costoAnterior = materiaPrima.CostoUnitario;
                var cantidadNueva = detalle.Cantidad;
                var costoNuevo = detalle.PrecioUnitario;

                var nuevoStock = stockAnterior + cantidadNueva;
                var nuevoCostoPromedio = nuevoStock > 0 ? 
                    (stockAnterior * costoAnterior + cantidadNueva * costoNuevo) / nuevoStock : costoNuevo;

                materiaPrima.Stock = nuevoStock;
                materiaPrima.CostoUnitario = nuevoCostoPromedio;
                
                await _context.SaveChangesAsync();

                // Actualizar precios en productos asociados
                await _precioActualizacionService.ActualizarPreciosProductosPorMateriaPrimaAsync(
                    materiaPrima.Id, nuevoCostoPromedio);

                // Verificar el nuevo estado después de la compra
                var materiaPrimaActualizada = await _context.MateriaPrima.FindAsync(materiaPrima.Id);
                var producto = await _context.ProductoHydroLink.FindAsync(11); // Producto "Prueba costeo"
                
                _logger.LogInformation("Estado después de la compra - Arduino Uno: Stock={Stock}, Costo={CostoUnitario}", 
                    materiaPrimaActualizada?.Stock, materiaPrimaActualizada?.CostoUnitario);
                _logger.LogInformation("Precio del producto 11 después de la compra: {Precio}", producto?.Precio);

                return Ok(new {
                    message = "Compra de prueba realizada",
                    estadoAnterior = new {
                        stock = stockAnterior,
                        costo = costoAnterior
                    },
                    estadoNuevo = new {
                        stock = materiaPrimaActualizada?.Stock,
                        costo = materiaPrimaActualizada?.CostoUnitario
                    },
                    precioProducto = producto?.Precio
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en compra de prueba");
                return BadRequest($"Error en compra de prueba: {ex.Message}");
            }
        }

        // Endpoint para verificar la cadena de actualización de precios
        [HttpGet("verificar-actualizacion-precios")]
        public async Task<ActionResult<object>> VerificarCadenaActualizacion()
        {
            try
            {
                // Obtener la materia prima, componente y producto
                var materiaPrima = await _context.MateriaPrima
                    .FirstOrDefaultAsync(mp => mp.Name.Contains("Arduino Uno"));
                
                if (materiaPrima == null)
                {
                    return BadRequest("No se encontró la materia prima Arduino Uno");
                }

                // Obtener relaciones de componente-materia prima
                var relaciones = await _context.ComponenteMateriaPrima
                    .Where(cmp => cmp.MateriaPrimaId == materiaPrima.Id && cmp.Activo)
                    .Include(cmp => cmp.Componente)
                    .ToListAsync();

                var componentesInfo = relaciones.Select(r => new {
                    componenteId = r.ComponenteId,
                    nombre = r.Componente.Nombre,
                    cantidadNecesaria = r.CantidadNecesaria,
                    porcentajeMerma = r.PorcentajeMerma,
                    cantidadConMerma = r.CantidadConMerma,
                    esPrincipal = r.EsPrincipal
                }).ToList();

                // Obtener productos que usan estos componentes
                var componenteIds = relaciones.Select(r => r.ComponenteId).ToList();
                var productosRelacionados = await _context.ComponenteRequerido
                    .Where(cr => componenteIds.Contains(cr.ComponenteId))
                    .Include(cr => cr.ProductoHydroLink)
                    .Select(cr => new {
                        productoId = cr.ProductoHydroLinkId,
                        nombreProducto = cr.ProductoHydroLink.Nombre,
                        precio = cr.ProductoHydroLink.Precio,
                        componenteId = cr.ComponenteId,
                        cantidadComponente = cr.Cantidad
                    })
                    .ToListAsync();

                // Calcular margen de ganancia del producto
                decimal margen = 0;
                var producto = await _context.ProductoHydroLink.FindAsync(11);
                if (producto != null)
                {
                    margen = await _precioActualizacionService.ObtenerMargenGananciaProductoAsync(producto.Id);
                }

                // Forzar la actualización de precios y ver el resultado
                decimal nuevoPrecio = 0;
                if (producto != null)
                {
                    nuevoPrecio = await _precioActualizacionService.RecalcularPrecioProductoConMargenExistenteAsync(producto.Id);
                }

                return Ok(new {
                    materiaPrima = new {
                        id = materiaPrima.Id,
                        nombre = materiaPrima.Name,
                        stock = materiaPrima.Stock,
                        costoUnitario = materiaPrima.CostoUnitario
                    },
                    componentesRelacionados = componentesInfo,
                    productosQueUsanComponentes = productosRelacionados,
                    producto11 = new {
                        id = producto?.Id,
                        nombre = producto?.Nombre,
                        precio = producto?.Precio,
                        margenGanancia = margen,
                        nuevoPrecioDespuesDeActualizacion = nuevoPrecio
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar la cadena de actualización de precios");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // Endpoint para probar la actualización directa de un precio
        [HttpPost("forzar-actualizacion/{productoId}")]
        public async Task<ActionResult<object>> ForzarActualizacionPrecio(int productoId)
        {
            try
            {
                var producto = await _context.ProductoHydroLink.FindAsync(productoId);
                if (producto == null)
                {
                    return NotFound($"Producto con ID {productoId} no encontrado");
                }

                var precioAnterior = producto.Precio;
                var nuevoPrecio = await _precioActualizacionService.RecalcularPrecioProductoConMargenExistenteAsync(productoId);

                return Ok(new {
                    productoId = producto.Id,
                    nombre = producto.Nombre,
                    precioAnterior = precioAnterior,
                    precioNuevo = nuevoPrecio,
                    resultado = "Precio actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al forzar actualización de precio para producto {ProductoId}", productoId);
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
