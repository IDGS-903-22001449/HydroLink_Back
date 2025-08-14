using HydroLink.Data;
using HydroLink.Dtos;
using HydroLink.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HydroLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/clientes
        [HttpGet]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _context.Persona.OfType<Cliente>()
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    c.Apellido,
                    c.Email,
                    c.Telefono,
                    c.Direccion,
                    c.Empresa,
                    c.FechaRegistro,
                    c.Activo
                })
                .ToListAsync();

            return Ok(clientes);
        }

        // GET: api/clientes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCliente(int id)
        {
            var cliente = await _context.Persona.OfType<Cliente>()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound($"Cliente con ID {id} no encontrado");

            var clienteDto = new
            {
                cliente.Id,
                cliente.Nombre,
                cliente.Apellido,
                cliente.Email,
                cliente.Telefono,
                cliente.Direccion,
                cliente.Empresa,
                cliente.FechaRegistro,
                cliente.Activo
            };

            return Ok(clienteDto);
        }

        // POST: api/clientes
        [HttpPost]
        public async Task<IActionResult> CrearCliente([FromBody] ClienteCreateDto createDto)
        {
            if (createDto == null)
                return BadRequest("Datos de cliente inválidos");

            var existeCliente = await _context.Persona.OfType<Cliente>()
                .AnyAsync(c => c.Email == createDto.Email);

            if (existeCliente)
                return BadRequest("Ya existe un cliente con este email");

            var cliente = new Cliente
            {
                Nombre = createDto.Nombre,
                Apellido = createDto.Apellido,
                Email = createDto.Email,
                Telefono = createDto.Telefono,
                Direccion = createDto.Direccion,
                Empresa = createDto.Empresa,
                FechaRegistro = DateTime.UtcNow,
                Activo = true
            };

            _context.Persona.Add(cliente);
            await _context.SaveChangesAsync();

            var clienteResponse = new
            {
                cliente.Id,
                cliente.Nombre,
                cliente.Apellido,
                cliente.Email,
                cliente.Telefono,
                cliente.Direccion,
                cliente.Empresa,
                cliente.FechaRegistro,
                cliente.Activo
            };

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, clienteResponse);
        }

        // PUT: api/clientes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCliente(int id, [FromBody] ClienteCreateDto updateDto)
        {
            var cliente = await _context.Persona.OfType<Cliente>()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound($"Cliente con ID {id} no encontrado");

            var existeOtroCliente = await _context.Persona.OfType<Cliente>()
                .AnyAsync(c => c.Email == updateDto.Email && c.Id != id);

            if (existeOtroCliente)
                return BadRequest("Ya existe otro cliente con este email");

            cliente.Nombre = updateDto.Nombre;
            cliente.Apellido = updateDto.Apellido;
            cliente.Email = updateDto.Email;
            cliente.Telefono = updateDto.Telefono;
            cliente.Direccion = updateDto.Direccion;
            cliente.Empresa = updateDto.Empresa;

            await _context.SaveChangesAsync();

            var clienteResponse = new
            {
                cliente.Id,
                cliente.Nombre,
                cliente.Apellido,
                cliente.Email,
                cliente.Telefono,
                cliente.Direccion,
                cliente.Empresa,
                cliente.FechaRegistro,
                cliente.Activo
            };

            return Ok(clienteResponse);
        }

        // DELETE: api/clientes/{id} (soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            var cliente = await _context.Persona.OfType<Cliente>()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound($"Cliente con ID {id} no encontrado");

            cliente.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cliente desactivado exitosamente" });
        }

        // GET: api/clientes/email/{email}
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetClientePorEmail(string email)
        {
            var cliente = await _context.Persona.OfType<Cliente>()
                .FirstOrDefaultAsync(c => c.Email == email);

            if (cliente == null)
                return NotFound($"Cliente con email {email} no encontrado");

            var clienteDto = new
            {
                cliente.Id,
                cliente.Nombre,
                cliente.Apellido,
                cliente.Email,
                cliente.Telefono,
                cliente.Direccion,
                cliente.Empresa,
                cliente.FechaRegistro,
                cliente.Activo
            };

            return Ok(clienteDto);
        }

        // GET: api/clientes/me - Obtener cliente del usuario actual autenticado
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetClienteActual()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            
            if (string.IsNullOrEmpty(userEmail))
                return BadRequest("No se pudo obtener el email del usuario autenticado");

            var cliente = await _context.Persona.OfType<Cliente>()
                .FirstOrDefaultAsync(c => c.Email == userEmail && c.Activo);

            if (cliente == null)
                return NotFound("No se encontró un cliente asociado a este usuario");

            var clienteDto = new
            {
                cliente.Id,
                cliente.Nombre,
                cliente.Apellido,
                cliente.Email,
                cliente.Telefono,
                cliente.Direccion,
                cliente.Empresa,
                cliente.FechaRegistro,
                cliente.Activo
            };

            return Ok(clienteDto);
        }
    }
}
