using HydroLink.Data;
using HydroLink.Dtos;
using HydroLink.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HydroLink.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProveedoresController> _logger;

        public ProveedoresController(AppDbContext context, ILogger<ProveedoresController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProveedorDto>>> GetProveedores()
        {
            try
            {
                var proveedores = await _context.Proveedor
                    .Select(p => new ProveedorDto
                    {
                        Id = p.Id,
                        NombreCompleto = (p.Nombre ?? "") + " " + (p.Apellido ?? ""),
                        Email = p.Email ?? "",
                        Telefono = p.Telefono ?? "",
                        Empresa = p.Empresa ?? ""
                    })
                    .ToListAsync();

                return Ok(proveedores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los proveedores");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Proveedor>> GetProveedor(int id)
        {
            try
            {
                var proveedor = await _context.Proveedor
                    .Include(p => p.Compras)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (proveedor == null)
                {
                    return NotFound(new { message = $"Proveedor con ID {id} no encontrado" });
                }

                return Ok(proveedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el proveedor con ID: {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Proveedor>> CreateProveedor(ProveedorCreateDto proveedorDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existeProveedor = await _context.Proveedor
                    .AnyAsync(p => p.Email == proveedorDto.Email);

                if (existeProveedor)
                {
                    return Conflict(new { message = "Ya existe un proveedor con este email" });
                }

                var proveedor = new Proveedor
                {
                    Nombre = proveedorDto.Nombre,
                    Apellido = proveedorDto.Apellido,
                    Email = proveedorDto.Email,
                    Telefono = proveedorDto.Telefono,
                    Direccion = proveedorDto.Direccion,
                    Empresa = proveedorDto.Empresa,
                    TipoPersona = "Proveedor"
                };

                _context.Proveedor.Add(proveedor);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProveedor), new { id = proveedor.Id }, proveedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el proveedor");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProveedor(int id, ProveedorCreateDto proveedorDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var proveedor = await _context.Proveedor.FindAsync(id);
                if (proveedor == null)
                {
                    return NotFound(new { message = $"Proveedor con ID {id} no encontrado" });
                }

                var existeEmailEnOtro = await _context.Proveedor
                    .AnyAsync(p => p.Email == proveedorDto.Email && p.Id != id);

                if (existeEmailEnOtro)
                {
                    return Conflict(new { message = "Ya existe otro proveedor con este email" });
                }
                proveedor.Nombre = proveedorDto.Nombre;
                proveedor.Apellido = proveedorDto.Apellido;
                proveedor.Email = proveedorDto.Email;
                proveedor.Telefono = proveedorDto.Telefono;
                proveedor.Direccion = proveedorDto.Direccion;
                proveedor.Empresa = proveedorDto.Empresa;

                _context.Entry(proveedor).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Proveedor actualizado exitosamente", proveedor });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el proveedor con ID: {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProveedor(int id)
        {
            try
            {
                var proveedor = await _context.Proveedor
                    .Include(p => p.Compras)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (proveedor == null)
                {
                    return NotFound(new { message = $"Proveedor con ID {id} no encontrado" });
                }

                if (proveedor.Compras?.Any() == true)
                {
                    return BadRequest(new { message = "No se puede eliminar el proveedor porque tiene compras asociadas" });
                }

                _context.Proveedor.Remove(proveedor);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Proveedor eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el proveedor con ID: {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProveedorDto>>> SearchProveedores([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(new { message = "El término de búsqueda no puede estar vacío" });
                }

                var proveedores = await _context.Proveedor
                    .Where(p => (p.Nombre != null && p.Nombre.Contains(searchTerm)) ||
                               (p.Apellido != null && p.Apellido.Contains(searchTerm)) ||
                               (p.Empresa != null && p.Empresa.Contains(searchTerm)) ||
                               (p.Email != null && p.Email.Contains(searchTerm)))
                    .Select(p => new ProveedorDto
                    {
                        Id = p.Id,
                        NombreCompleto = (p.Nombre ?? "") + " " + (p.Apellido ?? ""),
                        Email = p.Email ?? "",
                        Telefono = p.Telefono ?? "",
                        Empresa = p.Empresa ?? ""
                    })
                    .ToListAsync();

                return Ok(proveedores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar proveedores con término: {SearchTerm}", searchTerm);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}/compras")]
        public async Task<ActionResult<IEnumerable<object>>> GetComprasProveedor(int id)
        {
            try
            {
                var proveedor = await _context.Proveedor
                    .Include(p => p.Compras)
                    .ThenInclude(c => c.Detalles)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (proveedor == null)
                {
                    return NotFound(new { message = $"Proveedor con ID {id} no encontrado" });
                }

                var compras = proveedor.Compras.Select(c => new
                {
                    c.Id,
                    Fecha = c.Fecha,
                    Total = c.Detalles?.Sum(d => d.Cantidad * d.PrecioUnitario) ?? 0,
                    CantidadItems = c.Detalles?.Count ?? 0
                });

                return Ok(compras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las compras del proveedor con ID: {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}/estadisticas")]
        public async Task<ActionResult<object>> GetEstadisticasProveedor(int id)
        {
            try
            {
                var proveedor = await _context.Proveedor
                    .Include(p => p.Compras)
                    .ThenInclude(c => c.Detalles)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (proveedor == null)
                {
                    return NotFound(new { message = $"Proveedor con ID {id} no encontrado" });
                }

                var comprasConTotales = proveedor.Compras?.Select(c => new
                {
                    Fecha = c.Fecha,
                    Total = c.Detalles?.Sum(d => d.Cantidad * d.PrecioUnitario) ?? 0
                }).ToList();

                var estadisticas = new
                {
                    TotalCompras = proveedor.Compras?.Count ?? 0,
                    MontoTotalCompras = comprasConTotales?.Sum(c => c.Total) ?? 0,
                    PromedioCompra = comprasConTotales?.Any() == true ? comprasConTotales.Average(c => c.Total) : 0,
                    UltimaCompra = comprasConTotales?.OrderByDescending(c => c.Fecha).FirstOrDefault()?.Fecha,
                    PrimeraCompra = comprasConTotales?.OrderBy(c => c.Fecha).FirstOrDefault()?.Fecha
                };

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del proveedor con ID: {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("exists/{email}")]
        public async Task<ActionResult<object>> ExisteProveedorPorEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(new { message = "El email no puede estar vacío" });
                }

                var existe = await _context.Proveedor.AnyAsync(p => p.Email == email);
                
                return Ok(new { existe, email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar la existencia del proveedor con email: {Email}", email);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
