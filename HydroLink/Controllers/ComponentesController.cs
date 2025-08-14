
using HydroLink.Data;
using HydroLink.Dtos;
using HydroLink.Models;
using HydroLink.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HydroLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComponentesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPrecioComponenteService _precioService;
        private readonly ICostoPromedioService _costoPromedioService;
        private readonly IInventarioService _inventarioService;

        public ComponentesController(
            AppDbContext context, 
            IPrecioComponenteService precioService,
            ICostoPromedioService costoPromedioService,
            IInventarioService inventarioService)
        {
            _context = context;
            _precioService = precioService;
            _costoPromedioService = costoPromedioService;
            _inventarioService = inventarioService;
        }

        [HttpGet]
        public async Task<IActionResult> GetComponentes([FromQuery] string? categoria = null, [FromQuery] bool? soloActivos = null)
        {
            var query = _context.Componente.AsQueryable();

            if (soloActivos == true)
                query = query.Where(c => c.Activo);

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(c => c.Categoria.ToUpper() == categoria.ToUpper());

            var componentes = await query.ToListAsync();
            var componentesDto = new List<ComponenteDto>();

            foreach (var c in componentes)
            {
                var precioActual = await _precioService.ObtenerPrecioActualAsync(c.Id);
                componentesDto.Add(new ComponenteDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Categoria = c.Categoria,
                    UnidadMedida = c.UnidadMedida,
                    Especificaciones = c.Especificaciones,
                    EsPersonalizable = c.EsPersonalizable,
                    Activo = c.Activo
                });
            }

            return Ok(componentesDto);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetComponenteById(int id)
        {
            var componente = await _context.Componente.FirstOrDefaultAsync(c => c.Id == id);

            if (componente == null)
                return NotFound();

            var precioActual = await _precioService.ObtenerPrecioActualAsync(id);
            var componenteDto = new ComponenteDto
            {
                Id = componente.Id,
                Nombre = componente.Nombre,
                Descripcion = componente.Descripcion,
                Categoria = componente.Categoria,
                PrecioUnitario = precioActual,
                UnidadMedida = componente.UnidadMedida,
                Especificaciones = componente.Especificaciones,
                EsPersonalizable = componente.EsPersonalizable,
                Activo = componente.Activo
            };

            return Ok(componenteDto);
        }

        [HttpGet("{id}/precio-info")]
        public async Task<IActionResult> GetInfoPrecios(int id)
        {
            var info = await _precioService.ObtenerInfoPreciosAsync(id);
            return Ok(info);
        }

        [HttpPost]
        public async Task<IActionResult> CrearComponente([FromBody] ComponenteCreateDto createDto)
        {
            if (createDto == null)
                return BadRequest("Datos inválidos");

            var componente = new Componente
            {
                Nombre = createDto.Nombre,
                Descripcion = createDto.Descripcion,
                Categoria = createDto.Categoria.ToUpper(),
                UnidadMedida = createDto.UnidadMedida,
                Especificaciones = createDto.Especificaciones,
                EsPersonalizable = createDto.EsPersonalizable,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Componente.Add(componente);
            await _context.SaveChangesAsync();

            var precioActual = await _precioService.ObtenerPrecioActualAsync(componente.Id);
            var responseDto = new ComponenteDto
            {
                Id = componente.Id,
                Nombre = componente.Nombre,
                Descripcion = componente.Descripcion,
                Categoria = componente.Categoria,
                PrecioUnitario = precioActual,
                UnidadMedida = componente.UnidadMedida,
                Especificaciones = componente.Especificaciones,
                EsPersonalizable = componente.EsPersonalizable,
                Activo = componente.Activo
            };

            return CreatedAtAction(nameof(GetComponenteById), new { id = responseDto.Id }, responseDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarComponente(int id, [FromBody] ComponenteCreateDto updateDto)
        {
            var componente = await _context.Componente.FirstOrDefaultAsync(c => c.Id == id);

            if (componente == null)
                return NotFound();

            componente.Nombre = updateDto.Nombre;
            componente.Descripcion = updateDto.Descripcion;
            componente.Categoria = updateDto.Categoria.ToUpper();
            componente.UnidadMedida = updateDto.UnidadMedida;
            componente.Especificaciones = updateDto.Especificaciones;
            componente.EsPersonalizable = updateDto.EsPersonalizable;

            await _context.SaveChangesAsync();

            var precioActual = await _precioService.ObtenerPrecioActualAsync(id);
            var responseDto = new ComponenteDto
            {
                Id = componente.Id,
                Nombre = componente.Nombre,
                Descripcion = componente.Descripcion,
                Categoria = componente.Categoria,
                PrecioUnitario = precioActual,
                UnidadMedida = componente.UnidadMedida,
                Especificaciones = componente.Especificaciones,
                EsPersonalizable = componente.EsPersonalizable,
                Activo = componente.Activo
            };

            return Ok(responseDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarComponente(int id)
        {
            var componente = await _context.Componente.FirstOrDefaultAsync(c => c.Id == id);

            if (componente == null)
                return NotFound();

            componente.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _context.Componente
                .Where(c => c.Activo)
                .Select(c => c.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(categorias);
        }

        [HttpPost("configure-relationships")]
        public async Task<IActionResult> ConfigurarRelacionesAutomaticas()
        {
            try
            {
                var componentesMateriaPrima = await _context.Componente
                    .Where(c => c.Categoria == "MATERIA_PRIMA" && c.Activo)
                    .ToListAsync();
                
                var relacionesCreadas = 0;
                var detalles = new List<string>();
                
                foreach (var componente in componentesMateriaPrima)
                {
                    var tieneRelaciones = await _context.ComponenteMateriaPrima
                        .AnyAsync(cm => cm.ComponenteId == componente.Id && cm.Activo);
                    
                    if (!tieneRelaciones)
                    {
                        var materiaPrima = await _context.MateriaPrima
                            .FirstOrDefaultAsync(mp => mp.Name == componente.Nombre);
                        
                        if (materiaPrima != null)
                        {
                            var nuevaRelacion = new ComponenteMateriaPrima
                            {
                                ComponenteId = componente.Id,
                                MateriaPrimaId = materiaPrima.Id,
                                CantidadNecesaria = 1.0m,
                                FactorConversion = 1.0m,
                                PorcentajeMerma = 0.05m,
                                EsPrincipal = true,
                                Notas = "Relación 1:1 generada automáticamente",
                                Activo = true,
                                FechaCreacion = DateTime.UtcNow
                            };
                            
                            _context.ComponenteMateriaPrima.Add(nuevaRelacion);
                            relacionesCreadas++;
                            detalles.Add($"Componente '{componente.Nombre}' (ID: {componente.Id}) vinculado a MateriaPrima '{materiaPrima.Name}' (ID: {materiaPrima.Id})");
                        }
                        else
                        {
                            detalles.Add($"No se encontró materia prima para el componente '{componente.Nombre}' (ID: {componente.Id})");
                        }
                    }
                    else
                    {
                        detalles.Add($"Componente '{componente.Nombre}' (ID: {componente.Id}) ya tiene relaciones configuradas");
                    }
                }
                
                if (relacionesCreadas > 0)
                {
                    await _context.SaveChangesAsync();
                }
                
                return Ok(new 
                {
                    mensaje = "Relaciones configuradas",
                    relacionesCreadas = relacionesCreadas,
                    totalComponentes = componentesMateriaPrima.Count,
                    detalles = detalles
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
        
        [HttpGet("promedio-precio")]
        public async Task<IActionResult> GetPrecioPromedioPorCategoria([FromQuery] string categoria)
        {
            if (string.IsNullOrEmpty(categoria))
                return BadRequest("Categoría requerida");

            var componentesCategoria = await _context.Componente
                .Where(c => c.Activo && c.Categoria.ToUpper() == categoria.ToUpper())
                .ToListAsync();

            if (!componentesCategoria.Any())
                return NotFound($"No se encontraron componentes activos en la categoría {categoria}");

            var precios = new List<decimal>();
            foreach (var componente in componentesCategoria)
            {
                var precio = await _precioService.ObtenerPrecioPromedioAsync(componente.Id);
                if (precio > 0) precios.Add(precio);
            }

            if (!precios.Any())
                return NotFound($"No se encontraron precios para componentes en la categoría {categoria}");

            var precioPromedio = precios.Average();

            return Ok(new
            {
                Categoria = categoria.ToUpper(),
                PrecioPromedio = Math.Round(precioPromedio, 2),
                CantidadComponentes = componentesCategoria.Count,
                ComponentesConPrecios = precios.Count
            });
        }

        [HttpGet("{id}/costo-detalle")]
        public async Task<IActionResult> GetCostoDetalle(int id)
        {
            var detalle = await _costoPromedioService.ObtenerDetalleCostoComponenteAsync(id);
            if (string.IsNullOrEmpty(detalle.NombreComponente))
            {
                return NotFound(detalle.Observaciones);
            }
            return Ok(detalle);
        }

        [HttpGet("{id}/existencia")]
        public async Task<IActionResult> GetExistencia(int id)
        {
            var existencia = await _inventarioService.ObtenerExistenciaAsync(id);
            return Ok(new { ComponenteId = id, Existencia = existencia });
        }

        [HttpPost("{componenteId}/materias-primas")]
        public async Task<IActionResult> AgregarMateriaPrimaAComponente(int componenteId, [FromBody] ComponenteMateriaPrimaDto dto)
        {
            var componente = await _context.Componente.FindAsync(componenteId);
            var materiaPrima = await _context.MateriaPrima.FindAsync(dto.MateriaPrimaId);

            if (componente == null || materiaPrima == null)
                return NotFound("El componente o la materia prima no existen.");

            var relacion = new ComponenteMateriaPrima
            {
                ComponenteId = componenteId,
                MateriaPrimaId = dto.MateriaPrimaId,
                CantidadNecesaria = dto.CantidadNecesaria,
                FactorConversion = dto.FactorConversion,
                PorcentajeMerma = dto.PorcentajeMerma,
                EsPrincipal = dto.EsPrincipal,
                Notas = dto.Notas,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.ComponenteMateriaPrima.Add(relacion);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Materia prima agregada al componente exitosamente" });
        }
    }
}
