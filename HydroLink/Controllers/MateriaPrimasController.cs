using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HydroLink.Data;
using HydroLink.Models;
using Microsoft.AspNetCore.Authorization;
using HydroLink.Dtos;

namespace HydroLink.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MateriaPrimasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MateriaPrimasController> _logger;

        public MateriaPrimasController(AppDbContext context, ILogger<MateriaPrimasController> logger)
        {
            _context = context;
            _logger = logger; 
        }

        // GET: api/MateriaPrimas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MateriaPrimaDto>>> GetMateriaPrima()
        {
            try
            {
                var materiasPrimas = await _context.MateriaPrima.Select(m => new MateriaPrimaDto
                {
                    Id = m.Id,
                    Nombre = m.Name,
                    UnidadMedida = m.UnidadMedida
                })
                .ToListAsync();

                return Ok(materiasPrimas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las materias primas.");
                return BadRequest("No se pudieron obtener los registros.");
            }
        }
        
        // POST: api/MateriaPrimas/configure-relationships - Configurar relaciones automáticas
        [HttpPost("configure-relationships")]
        public async Task<IActionResult> ConfigurarRelacionesAutomaticas()
        {
            try
            {
                var materiasPrimas = await _context.MateriaPrima.ToListAsync();
                var relacionesCreadas = 0;
                
                foreach (var mp in materiasPrimas)
                {
                    var componente = await _context.Componente
                        .FirstOrDefaultAsync(c => c.Nombre == mp.Name && c.UnidadMedida == mp.UnidadMedida);
                    
                    if (componente == null)
                    {
                        componente = new Componente
                        {
                            Nombre = mp.Name,
                            Descripcion = $"Componente generado automáticamente para {mp.Name}",
                            Categoria = "MATERIA_PRIMA",
                            UnidadMedida = mp.UnidadMedida,
                            Especificaciones = "Generado automáticamente",
                            EsPersonalizable = false,
                            Activo = true,
                            FechaCreacion = DateTime.UtcNow
                        };
                        
                        _context.Componente.Add(componente);
                        await _context.SaveChangesAsync(); 
                    }
                    
                    var relacionExistente = await _context.ComponenteMateriaPrima
                        .FirstOrDefaultAsync(cm => cm.ComponenteId == componente.Id && cm.MateriaPrimaId == mp.Id);
                    
                    if (relacionExistente == null)
                    {
                        var nuevaRelacion = new ComponenteMateriaPrima
                        {
                            ComponenteId = componente.Id,
                            MateriaPrimaId = mp.Id,
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
                    }
                }
                
                await _context.SaveChangesAsync();
                
                return Ok(new 
                {
                    mensaje = "Relaciones configuradas exitosamente",
                    relacionesCreadas = relacionesCreadas,
                    totalMateriasPrimas = materiasPrimas.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al configurar relaciones automáticas");
                return BadRequest("No se pudieron configurar las relaciones automáticas");
            }
        }
        
        // GET: api/MateriaPrimas/as-components 
        [HttpGet("as-components")]
        public async Task<ActionResult<IEnumerable<ComponenteDto>>> GetMateriaPrimasAsComponents()
        {
            try
            {
                var materiasPrimas = await _context.MateriaPrima.ToListAsync();
                
                foreach (var mp in materiasPrimas)
                {
                    var componenteExistente = await _context.Componente
                        .FirstOrDefaultAsync(c => c.Nombre == mp.Name && c.UnidadMedida == mp.UnidadMedida);
                    
                    if (componenteExistente == null)
                    {
                        var nuevoComponente = new Componente
                        {
                            Nombre = mp.Name,
                            Descripcion = $"Componente generado automáticamente para {mp.Name}",
                            Categoria = "MATERIA_PRIMA",
                            UnidadMedida = mp.UnidadMedida,
                            Especificaciones = "Generado automáticamente",
                            EsPersonalizable = false,
                            Activo = true,
                            FechaCreacion = DateTime.UtcNow
                        };
                        
                        _context.Componente.Add(nuevoComponente);
                    }
                }
                
                await _context.SaveChangesAsync();
                
                var componentesMateriaPrima = await _context.Componente
                    .Where(c => c.Categoria == "MATERIA_PRIMA" && c.Activo)
                    .Select(c => new ComponenteDto
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion,
                        Categoria = c.Categoria,
                        UnidadMedida = c.UnidadMedida,
                        Especificaciones = c.Especificaciones,
                        EsPersonalizable = c.EsPersonalizable,
                        Activo = c.Activo
                    })
                    .ToListAsync();

                return Ok(componentesMateriaPrima);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las materias primas como componentes.");
                return BadRequest("No se pudieron obtener los registros.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MateriaPrimaDto>> GetMateriaPrima(int id)
        {
            try
            {
                var materiaPrima = await _context.MateriaPrima.FindAsync(id);
                if (materiaPrima == null)
                    return NotFound(new { message = "No se encontro la materia prima." });
                {

                    var dto = new MateriaPrimaDto
                    {
                        Id = materiaPrima.Id,
                        Nombre = materiaPrima.Name,
                        UnidadMedida = materiaPrima.UnidadMedida
                    };
                    return Ok(dto);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "No se encotró la materia prima");
                return BadRequest("Hubo un prblema al buscar la materia prima");
            }
        }

        // POST: api/MateriaPrimas
        [HttpPost]
        public async Task<ActionResult<MateriaPrima>> NuevaMateriaPrima([FromBody] MateriaPrimaCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var materia = new MateriaPrima
                {
                    Name = dto.Nombre,
                    UnidadMedida = dto.UnidadMedida,
                    CostoUnitario = 0,
                    Stock = 0
                };

                _context.MateriaPrima.Add(materia);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetMateriaPrima), new { id = materia.Id }, materia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se agregó la materia prima");
                return BadRequest("Hubo un problema al insertar la materia prima");
            }
        }


        // PUT: api/MateriaPrimas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarMateriaPrima(int id, [FromBody] MateriaPrimaCreateDto dto)
        {
            try
            {
                var materia = await _context.MateriaPrima.FindAsync(id);
                if (materia == null)
                    return NotFound();

                materia.Name = dto.Nombre;
                materia.UnidadMedida = dto.UnidadMedida;

                _context.Entry(materia).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se editó la materia prima");
                return BadRequest("Hubo un problema al modificar la materia prima");
            }
        }


        // DELETE: api/MateriaPrimas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarMateriaPrima(int id)
        {
            try
            {
                var materiaPrima = await _context.MateriaPrima.FindAsync(id);
                if (materiaPrima == null)
                {
                    return NotFound(new { message = "No se encontró la materia prima especificada." });
                }

                var tieneRelacionesComponentes = await _context.ComponenteMateriaPrima
                    .AnyAsync(cm => cm.MateriaPrimaId == id && cm.Activo);

                if (tieneRelacionesComponentes)
                {
                    return BadRequest(new 
                    { 
                        message = "No se puede eliminar la materia prima porque está siendo utilizada por uno o más componentes.",
                        detail = "Elimine primero las relaciones con componentes o desactive los componentes que la utilizan."
                    });
                }

                var tieneMovimientosInventario = await _context.MovimientoInventario
                    .AnyAsync(mi => mi.MateriaPrimaId == id);

                if (tieneMovimientosInventario)
                {
                    return BadRequest(new 
                    { 
                        message = "No se puede eliminar la materia prima porque tiene movimientos de inventario registrados.",
                        detail = "Las materias primas con historial de inventario no pueden ser eliminadas por integridad de datos."
                    });
                }

                if (materiaPrima.Stock > 0)
                {
                    return BadRequest(new 
                    { 
                        message = $"No se puede eliminar la materia prima porque tiene stock actual de {materiaPrima.Stock} {materiaPrima.UnidadMedida}.",
                        detail = "Reduzca el stock a cero antes de eliminar la materia prima."
                    });
                }

                _context.MateriaPrima.Remove(materiaPrima);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Materia prima eliminada exitosamente: {materiaPrima.Name} (ID: {id})");

                return Ok(new 
                { 
                    message = "Materia prima eliminada exitosamente.",
                    materiaPrimaEliminada = new
                    {
                        Id = materiaPrima.Id,
                        Nombre = materiaPrima.Name,
                        UnidadMedida = materiaPrima.UnidadMedida
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar la materia prima con ID: {id}");
                return BadRequest(new 
                { 
                    message = "Hubo un problema al eliminar la materia prima.",
                    detail = "Error interno del servidor. Revise los logs para más detalles."
                });
            }
        }

        // GET: api/MateriaPrimas/debug-inventory 
        [HttpGet("debug-inventory")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugInventory()
        {
            try
            {
                var materiasPrimas = await _context.MateriaPrima
                    .Select(mp => new
                    {
                        Id = mp.Id,
                        Nombre = mp.Name,
                        Stock = mp.Stock,
                        CostoUnitario = mp.CostoUnitario
                    })
                    .ToListAsync();

                var componentes = await _context.Componente
                    .Where(c => c.Categoria == "MATERIA_PRIMA" && c.Activo)
                    .Select(c => new
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Categoria = c.Categoria
                    })
                    .ToListAsync();

                var relaciones = await _context.ComponenteMateriaPrima
                    .Where(cm => cm.Activo)
                    .Include(cm => cm.Componente)
                    .Include(cm => cm.MateriaPrima)
                    .Select(cm => new
                    {
                        Id = cm.Id,
                        ComponenteId = cm.ComponenteId,
                        NombreComponente = cm.Componente.Nombre,
                        MateriaPrimaId = cm.MateriaPrimaId,
                        NombreMateriaPrima = cm.MateriaPrima.Name,
                        CantidadNecesaria = cm.CantidadNecesaria,
                        CantidadConMerma = cm.CantidadConMerma,
                        PorcentajeMerma = cm.PorcentajeMerma
                    })
                    .ToListAsync();

                var movimientosComponente = await _context.MovimientoComponente
                    .Include(mc => mc.Componente)
                    .Select(mc => new
                    {
                        Id = mc.Id,
                        ComponenteId = mc.ComponenteId,
                        NombreComponente = mc.Componente.Nombre,
                        TipoMovimiento = mc.TipoMovimiento,
                        Cantidad = mc.Cantidad,
                        FechaMovimiento = mc.FechaMovimiento
                    })
                    .OrderByDescending(mc => mc.FechaMovimiento)
                    .Take(20)
                    .ToListAsync();

                return Ok(new
                {
                    MateriasPrimas = materiasPrimas,
                    Componentes = componentes,
                    Relaciones = relaciones,
                    MovimientosComponente = movimientosComponente
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información de depuración del inventario");
                return BadRequest("Error al obtener información de depuración");
            }
        }

        // POST: api/MateriaPrimas/seed-test-data 
        [HttpPost("seed-test-data")]
        [AllowAnonymous]
        public async Task<IActionResult> SeedTestData()
        {
            try
            {
                var datosCreados = new List<string>();

                if (!await _context.MateriaPrima.AnyAsync())
                {
                    var materiasPrimas = new List<MateriaPrima>
                    {
                        new MateriaPrima { Name = "Sensor IoT", UnidadMedida = "unidad", Stock = 50, CostoUnitario = 15.00m },
                        new MateriaPrima { Name = "Válvula Electrónica", UnidadMedida = "unidad", Stock = 30, CostoUnitario = 25.00m },
                        new MateriaPrima { Name = "Tubo PVC", UnidadMedida = "metro", Stock = 100, CostoUnitario = 5.00m },
                        new MateriaPrima { Name = "Bomba de Agua", UnidadMedida = "unidad", Stock = 20, CostoUnitario = 45.00m },
                        new MateriaPrima { Name = "Panel de Control", UnidadMedida = "unidad", Stock = 15, CostoUnitario = 80.00m }
                    };

                    _context.MateriaPrima.AddRange(materiasPrimas);
                    await _context.SaveChangesAsync();
                    datosCreados.Add($"Creadas {materiasPrimas.Count} materias primas");
                }

                
                await ConfigurarRelacionesAutomaticas();
                datosCreados.Add("Configuradas relaciones componente-materia prima");

                if (!await _context.Persona.OfType<Cliente>().AnyAsync())
                {
                    var clientes = new List<Cliente>
                    {
                        new Cliente { Nombre = "Juan", Apellido = "Pérez", Email = "juan@example.com", Telefono = "555-0001", Direccion = "Calle 123", TipoPersona = "Cliente" },
                        new Cliente { Nombre = "María", Apellido = "García", Email = "maria@example.com", Telefono = "555-0002", Direccion = "Calle 456", TipoPersona = "Cliente" },
                        new Cliente { Nombre = "Carlos", Apellido = "López", Email = "carlos@example.com", Telefono = "555-0003", Direccion = "Calle 789", TipoPersona = "Cliente" }
                    };

                    _context.Persona.AddRange(clientes);
                    await _context.SaveChangesAsync();
                    datosCreados.Add($"Creados {clientes.Count} clientes");
                }

                if (!await _context.ProductoHydroLink.AnyAsync())
                {
                    var productos = new List<ProductoHydroLink>
                    {
                        new ProductoHydroLink 
                        { 
                            Nombre = "Sistema HydroLink Basic", 
                            Precio = 299.99m, 
                            Categoria = "Residencial", 
                            Descripcion = "Sistema básico de riego automatizado",
                            Especificaciones = "Incluye 2 sensores y 1 válvula",
                            Activo = true
                        },
                        new ProductoHydroLink 
                        { 
                            Nombre = "Sistema HydroLink Pro", 
                            Precio = 599.99m, 
                            Categoria = "Comercial", 
                            Descripcion = "Sistema profesional de riego IoT",
                            Especificaciones = "Incluye 4 sensores, 2 válvulas y panel de control",
                            Activo = true
                        },
                        new ProductoHydroLink 
                        { 
                            Nombre = "Sistema HydroLink Industrial", 
                            Precio = 1299.99m, 
                            Categoria = "Industrial", 
                            Descripcion = "Sistema industrial completo",
                            Especificaciones = "Incluye 6 sensores, 4 válvulas, bomba y panel",
                            Activo = true
                        }
                    };

                    _context.ProductoHydroLink.AddRange(productos);
                    await _context.SaveChangesAsync();
                    datosCreados.Add($"Creados {productos.Count} productos HydroLink");

                    var sensorComponent = await _context.Componente.FirstOrDefaultAsync(c => c.Nombre == "Sensor IoT");
                    var valvulaComponent = await _context.Componente.FirstOrDefaultAsync(c => c.Nombre == "Válvula Electrónica");
                    var bombaComponent = await _context.Componente.FirstOrDefaultAsync(c => c.Nombre == "Bomba de Agua");
                    var panelComponent = await _context.Componente.FirstOrDefaultAsync(c => c.Nombre == "Panel de Control");

                    var componentesRequeridos = new List<ComponenteRequerido>();

                    if (sensorComponent != null && valvulaComponent != null)
                    {
                        componentesRequeridos.AddRange(new[]
                        {
                            new ComponenteRequerido { ProductoHydroLinkId = productos[0].Id, ComponenteId = sensorComponent.Id, Cantidad = 2 },
                            new ComponenteRequerido { ProductoHydroLinkId = productos[0].Id, ComponenteId = valvulaComponent.Id, Cantidad = 1 }
                        });

                        componentesRequeridos.AddRange(new[]
                        {
                            new ComponenteRequerido { ProductoHydroLinkId = productos[1].Id, ComponenteId = sensorComponent.Id, Cantidad = 4 },
                            new ComponenteRequerido { ProductoHydroLinkId = productos[1].Id, ComponenteId = valvulaComponent.Id, Cantidad = 2 }
                        });
                        
                        if (panelComponent != null)
                            componentesRequeridos.Add(new ComponenteRequerido { ProductoHydroLinkId = productos[1].Id, ComponenteId = panelComponent.Id, Cantidad = 1 });
 
                        componentesRequeridos.AddRange(new[]
                        {
                            new ComponenteRequerido { ProductoHydroLinkId = productos[2].Id, ComponenteId = sensorComponent.Id, Cantidad = 6 },
                            new ComponenteRequerido { ProductoHydroLinkId = productos[2].Id, ComponenteId = valvulaComponent.Id, Cantidad = 4 }
                        });
                        
                        if (bombaComponent != null)
                            componentesRequeridos.Add(new ComponenteRequerido { ProductoHydroLinkId = productos[2].Id, ComponenteId = bombaComponent.Id, Cantidad = 1 });
                        
                        if (panelComponent != null)
                            componentesRequeridos.Add(new ComponenteRequerido { ProductoHydroLinkId = productos[2].Id, ComponenteId = panelComponent.Id, Cantidad = 1 });
                    }

                    if (componentesRequeridos.Any())
                    {
                        _context.ComponenteRequerido.AddRange(componentesRequeridos);
                        await _context.SaveChangesAsync();
                        datosCreados.Add($"Configurados {componentesRequeridos.Count} componentes requeridos para productos");
                    }
                }

                return Ok(new
                {
                    mensaje = "Datos de prueba creados exitosamente",
                    datosCreados = datosCreados
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear datos de prueba");
                return BadRequest(new { mensaje = "Error al crear datos de prueba", detalle = ex.Message });
            }
        }
    }
}
