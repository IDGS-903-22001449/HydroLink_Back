using HydroLink.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HydroLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductosPreciosController : ControllerBase
    {
        private readonly IProductoPrecioService _productoPrecioService;

        public ProductosPreciosController(IProductoPrecioService productoPrecioService)
        {
            _productoPrecioService = productoPrecioService;
        }

        // GET: api/productosprecios/{id}/calcular
        [HttpGet("{id}/calcular")]
        public async Task<IActionResult> CalcularPrecio(int id, decimal margenGanancia = 0.30m)
        {
            try
            {
                var precio = await _productoPrecioService.CalcularPrecioProductoAsync(id, margenGanancia);
                return Ok(new { productoId = id, precioCalculado = precio, margenGanancia });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al calcular precio", detalle = ex.Message });
            }
        }

        // GET: api/productosprecios/{id}/detalle
        [HttpGet("{id}/detalle")]
        public async Task<IActionResult> ObtenerDetallePrecio(int id, decimal margenGanancia = 0.30m)
        {
            try
            {
                var detalle = await _productoPrecioService.ObtenerDetalleCalculoPrecioAsync(id, margenGanancia);
                return Ok(detalle);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al obtener detalle de precio", detalle = ex.Message });
            }
        }

        // POST: api/productosprecios/{id}/actualizar
        [HttpPost("{id}/actualizar")]
        public async Task<IActionResult> ActualizarPrecio(int id, decimal margenGanancia = 0.30m)
        {
            try
            {
                var actualizado = await _productoPrecioService.ActualizarPrecioProductoAsync(id, margenGanancia);
                if (actualizado)
                {
                    return Ok(new { mensaje = "Precio actualizado exitosamente", productoId = id });
                }
                else
                {
                    return NotFound(new { mensaje = "Producto no encontrado" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al actualizar precio", detalle = ex.Message });
            }
        }

        // POST: api/productosprecios/actualizar-todos
        [HttpPost("actualizar-todos")]
        public async Task<IActionResult> ActualizarTodosLosPrecios(decimal margenGanancia = 0.30m)
        {
            try
            {
                var actualizados = await _productoPrecioService.ActualizarTodosLosPreciosAsync(margenGanancia);
                return Ok(new { mensaje = $"Se actualizaron {actualizados} productos", productosActualizados = actualizados });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al actualizar precios", detalle = ex.Message });
            }
        }

        // POST: api/productosprecios/recalcular-despues-compra
        [HttpPost("recalcular-despues-compra")]
        public async Task<IActionResult> RecalcularDespuesDeCompra([FromBody] List<int> componentesAfectados, decimal margenGanancia = 0.30m)
        {
            try
            {
                var actualizados = await _productoPrecioService.RecalcularPreciosDespuesDeCompraAsync(componentesAfectados, margenGanancia);
                return Ok(new { mensaje = $"Se recalcularon {actualizados} productos", productosAfectados = actualizados });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al recalcular precios", detalle = ex.Message });
            }
        }
    }
}
