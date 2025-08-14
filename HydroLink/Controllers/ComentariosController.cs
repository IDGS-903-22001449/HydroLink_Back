using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HydroLink.Data;
using HydroLink.Models;
using HydroLink.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace HydroLink.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComentariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ComentariosController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Comentarios
        [HttpGet]
        public async Task<ActionResult<object>> GetComentarios(
            [FromQuery] int? productoHydroLinkId = null,
            [FromQuery] int? calificacion = null,
            [FromQuery] string ordenamiento = "recientes",
            [FromQuery] int pagina = 1,
            [FromQuery] int tamañoPagina = 10)
        {
            var query = _context.Comentario
                .Include(c => c.Usuario)
                .Include(c => c.ProductoHydroLink)
                .AsQueryable();

            if (productoHydroLinkId.HasValue)
            {
                query = query.Where(c => c.ProductoHydroLinkId == productoHydroLinkId.Value);
            }

            if (calificacion.HasValue)
            {
                query = query.Where(c => c.Calificacion == calificacion.Value);
            }

            query = ordenamiento.ToLower() switch
            {
                "antiguos" => query.OrderBy(c => c.Fecha),
                "mejorvaloracion" => query.OrderByDescending(c => c.Calificacion).ThenByDescending(c => c.Fecha),
                "peorvaloracion" => query.OrderBy(c => c.Calificacion).ThenByDescending(c => c.Fecha),
                _ => query.OrderByDescending(c => c.Fecha)
            };

            var totalComentarios = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling((double)totalComentarios / tamañoPagina);

            var comentarios = await query
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .Select(c => new
                {
                    c.Id,
                    c.Texto,
                    c.Calificacion,
                    c.Fecha,
                    c.ProductoHydroLinkId,
                    ProductoNombre = c.ProductoHydroLink.Nombre,
                    Usuario = new
                    {
                        c.Usuario.Id,
                        c.Usuario.FullName,
                        c.Usuario.Email
                    }
                })
                .ToListAsync();

            return Ok(new
            {
                Comentarios = comentarios,
                Paginacion = new
                {
                    PaginaActual = pagina,
                    TotalPaginas = totalPaginas,
                    TotalComentarios = totalComentarios,
                    TamañoPagina = tamañoPagina
                }
            });
        }

        // GET: api/Comentarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetComentario(int id)
        {
            var comentario = await _context.Comentario
                .Include(c => c.Usuario)
                .Include(c => c.ProductoHydroLink)
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Texto,
                    c.Calificacion,
                    c.Fecha,
                    c.ProductoHydroLinkId,
                    ProductoNombre = c.ProductoHydroLink.Nombre,
                    Usuario = new
                    {
                        c.Usuario.Id,
                        c.Usuario.FullName,
                        c.Usuario.Email
                    }
                })
                .FirstOrDefaultAsync();

            if (comentario == null)
            {
                return NotFound();
            }

            return Ok(comentario);
        }

        // GET: api/Comentarios/ProductoHydroLink/5
        [HttpGet("ProductoHydroLink/{productoHydroLinkId}")]
        public async Task<ActionResult<object>> GetComentariosPorProductoHydroLink(int productoHydroLinkId)
        {
            var comentarios = await _context.Comentario
                .Where(c => c.ProductoHydroLinkId == productoHydroLinkId)
                .Include(c => c.Usuario)
                .Include(c => c.ProductoHydroLink)
                .Select(c => new
                {
                    c.Id,
                    c.Texto,
                    c.Calificacion,
                    c.Fecha,
                    c.ProductoHydroLinkId,
                    ProductoNombre = c.ProductoHydroLink.Nombre,
                    Usuario = new
                    {
                        c.Usuario.Id,
                        c.Usuario.FullName,
                        c.Usuario.Email
                    }
                })
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

            // Calcular estadísticas
            var totalComentarios = comentarios.Count();
            var promedioCalificacion = totalComentarios > 0 ? comentarios.Average(c => c.Calificacion) : 0;
            var distribucionCalificaciones = comentarios
                .GroupBy(c => c.Calificacion)
                .ToDictionary(g => g.Key, g => g.Count());

            var resultado = new
            {
                Comentarios = comentarios,
                Estadisticas = new
                {
                    TotalComentarios = totalComentarios,
                    PromedioCalificacion = Math.Round(promedioCalificacion, 2),
                    DistribucionCalificaciones = distribucionCalificaciones
                }
            };

            return Ok(resultado);
        }

        // GET: api/Comentarios/Usuario/5
        [HttpGet("Usuario/{usuarioId}")]
        [Authorize]
        public async Task<ActionResult<object>> GetComentariosPorUsuario(string usuarioId)
        {
            var comentarios = await _context.Comentario
                .Where(c => c.UsuarioId == usuarioId)
                .Include(c => c.Usuario)
                .Include(c => c.ProductoHydroLink)
                .Select(c => new
                {
                    c.Id,
                    c.Texto,
                    c.Calificacion,
                    c.Fecha,
                    c.ProductoHydroLinkId,
                    ProductoNombre = c.ProductoHydroLink.Nombre,
                    Usuario = new
                    {
                        c.Usuario.Id,
                        c.Usuario.FullName,
                        c.Usuario.Email
                    }
                })
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

            return Ok(comentarios);
        }

        // GET: api/Comentarios/Estadisticas/Globales
        [HttpGet("Estadisticas/Globales")]
        public async Task<ActionResult<object>> GetEstadisticasGlobales()
        {
            var totalComentarios = await _context.Comentario.CountAsync();
            var promedioGeneral = totalComentarios > 0 ? 
                await _context.Comentario.AverageAsync(c => (double)c.Calificacion) : 0;

            var distribucionCalificaciones = await _context.Comentario
                .GroupBy(c => c.Calificacion)
                .Select(g => new { Calificacion = g.Key, Cantidad = g.Count() })
                .OrderBy(x => x.Calificacion)
                .ToListAsync();

            var productosConMasComentarios = await _context.Comentario
                .Include(c => c.ProductoHydroLink)
                .GroupBy(c => new { c.ProductoHydroLinkId, c.ProductoHydroLink.Nombre })
                .Select(g => new 
                { 
                    ProductoHydroLinkId = g.Key.ProductoHydroLinkId,
                    ProductoNombre = g.Key.Nombre,
                    CantidadComentarios = g.Count(),
                    PromedioCalificacion = Math.Round(g.Average(c => (double)c.Calificacion), 2)
                })
                .OrderByDescending(p => p.CantidadComentarios)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                TotalComentarios = totalComentarios,
                PromedioGeneral = Math.Round(promedioGeneral, 2),
                DistribucionCalificaciones = distribucionCalificaciones,
                ProductosConMasComentarios = productosConMasComentarios
            });
        }

        // GET: api/Comentarios/ProductosHydroLink
        [HttpGet("ProductosHydroLink")]
        public async Task<ActionResult<object>> GetProductosHydroLinkConComentarios()
        {
            var productos = await _context.ProductoHydroLink
                .Where(p => _context.Comentario.Any(c => c.ProductoHydroLinkId == p.Id))
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    CantidadComentarios = _context.Comentario.Count(c => c.ProductoHydroLinkId == p.Id),
                    PromedioCalificacion = _context.Comentario.Any(c => c.ProductoHydroLinkId == p.Id) ? 
                        Math.Round(_context.Comentario.Where(c => c.ProductoHydroLinkId == p.Id).Average(c => (double)c.Calificacion), 2) : 0
                })
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return Ok(productos);
        }

        // TEMPORAL: GET: api/Comentarios/UsuariosDisponibles
        [HttpGet("UsuariosDisponibles")]
        public async Task<ActionResult<object>> GetUsuariosDisponibles()
        {
            var usuarios = await _context.Usuario
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.Email
                })
                .ToListAsync();

            return Ok(new
            {
                TotalUsuarios = usuarios.Count,
                Usuarios = usuarios
            });
        }

        // TEMPORAL: GET: api/Comentarios/ProductosDisponibles
        [HttpGet("ProductosDisponibles")]
        public async Task<ActionResult<object>> GetProductosDisponibles()
        {
            var productosRegulares = await _context.Producto
                .Select(p => new
                {
                    Tabla = "Producto",
                    p.Id,
                    p.Nombre,
                    p.Descripcion,
                    Precio = p.PrecioVenta
                })
                .OrderBy(p => p.Id)
                .ToListAsync();

            var productosHydroLink = await _context.ProductoHydroLink
                .Select(p => new
                {
                    Tabla = "ProductoHydroLink",
                    p.Id,
                    p.Nombre,
                    p.Descripcion,
                    p.Precio
                })
                .OrderBy(p => p.Id)
                .ToListAsync();

            return Ok(new
            {
                TotalProductosRegulares = productosRegulares.Count,
                ProductosRegulares = productosRegulares,
                TotalProductosHydroLink = productosHydroLink.Count,
                ProductosHydroLink = productosHydroLink
            });
        }

        // POST: api/Comentarios
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<object>> PostComentario([FromBody] ComentarioCreateDto comentarioDto)
        {
            try
            {
                var usuario = await _userManager.FindByIdAsync(comentarioDto.UsuarioId);
                if (usuario == null)
                {
                    return BadRequest($"Usuario no encontrado con ID: {comentarioDto.UsuarioId}");
                }

                var productoHydroLink = await _context.ProductoHydroLink.FindAsync(comentarioDto.ProductoHydroLinkId);
                if (productoHydroLink == null)
                {
                    return BadRequest($"Producto HydroLink no encontrado con ID: {comentarioDto.ProductoHydroLinkId}");
                }

                var comentarioExistente = await _context.Comentario
                    .FirstOrDefaultAsync(c => c.UsuarioId == comentarioDto.UsuarioId && c.ProductoHydroLinkId == comentarioDto.ProductoHydroLinkId);
                
                if (comentarioExistente != null)
                {
                    return BadRequest("Ya has comentado este producto. Puedes editar tu comentario existente.");
                }

                var comentario = new Comentario
                {
                    UsuarioId = comentarioDto.UsuarioId,
                    ProductoHydroLinkId = comentarioDto.ProductoHydroLinkId,
                    Texto = comentarioDto.Texto,
                    Calificacion = comentarioDto.Calificacion,
                    Fecha = DateTime.UtcNow
                };

                _context.Comentario.Add(comentario);
                await _context.SaveChangesAsync();

                var comentarioResponse = await _context.Comentario
                    .Include(c => c.Usuario)
                    .Include(c => c.ProductoHydroLink)
                    .Where(c => c.Id == comentario.Id)
                    .Select(c => new
                    {
                        c.Id,
                        c.Texto,
                        c.Calificacion,
                        c.Fecha,
                        c.ProductoHydroLinkId,
                        ProductoNombre = c.ProductoHydroLink.Nombre,
                        Usuario = new
                        {
                            c.Usuario.Id,
                            c.Usuario.FullName,
                            c.Usuario.Email
                        }
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction("GetComentario", new { id = comentario.Id }, comentarioResponse);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // PUT: api/Comentarios/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutComentario(int id, ComentarioCreateDto comentarioDto)
        {
            var comentarioExistente = await _context.Comentario.FindAsync(id);
            if (comentarioExistente == null)
            {
                return NotFound();
            }

            if (comentarioExistente.UsuarioId != comentarioDto.UsuarioId)
            {
                return Forbid("No tienes permisos para editar este comentario");
            }

            comentarioExistente.Texto = comentarioDto.Texto;
            comentarioExistente.Calificacion = comentarioDto.Calificacion;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComentarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Comentarios/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComentario(int id)
        {
            var comentario = await _context.Comentario.FindAsync(id);
            if (comentario == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            if (comentario.UsuarioId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid("No tienes permisos para eliminar este comentario");
            }

            _context.Comentario.Remove(comentario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ComentarioExists(int id)
        {
            return _context.Comentario.Any(e => e.Id == id);
        }
    }
}
