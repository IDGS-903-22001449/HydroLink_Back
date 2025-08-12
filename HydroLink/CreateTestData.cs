using HydroLink.Data;
using HydroLink.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace HydroLink
{
    public class CreateTestData
    {
        public static async Task CreateTestDataAsync(AppDbContext context, UserManager<AppUser> userManager)
        {
            try
            {
                // Buscar el usuario vicente@gmail.com
                var user = await userManager.FindByEmailAsync("vicente@gmail.com");
                if (user == null)
                {
                    Console.WriteLine("Usuario vicente@gmail.com no encontrado");
                    return;
                }

                // Obtener algunos productos existentes
                var productos = await context.ProductoHydroLink
                    .Where(p => p.Activo)
                    .Take(2)
                    .ToListAsync();

                if (!productos.Any())
                {
                    Console.WriteLine("No se encontraron productos activos");
                    return;
                }

                var primerProducto = productos.First();
                var segundoProducto = productos.Skip(1).FirstOrDefault();

                // Crear compras de prueba si no existen
                var compra1Existe = await context.ProductoComprado
                    .AnyAsync(pc => pc.UserId == user.Id && pc.ProductoId == primerProducto.Id);

                if (!compra1Existe)
                {
                    var compra1 = new ProductoComprado
                    {
                        UserId = user.Id,
                        ProductoId = primerProducto.Id,
                        FechaCompra = DateTime.UtcNow.AddDays(-30),
                        VentaId = null
                    };
                    context.ProductoComprado.Add(compra1);
                    Console.WriteLine($"Compra 1 creada para producto: {primerProducto.Nombre}");
                }

                if (segundoProducto != null)
                {
                    var compra2Existe = await context.ProductoComprado
                        .AnyAsync(pc => pc.UserId == user.Id && pc.ProductoId == segundoProducto.Id);

                    if (!compra2Existe)
                    {
                        var compra2 = new ProductoComprado
                        {
                            UserId = user.Id,
                            ProductoId = segundoProducto.Id,
                            FechaCompra = DateTime.UtcNow.AddDays(-15),
                            VentaId = null
                        };
                        context.ProductoComprado.Add(compra2);
                        Console.WriteLine($"Compra 2 creada para producto: {segundoProducto.Nombre}");
                    }
                }

                // Añadir un manual PDF de prueba al primer producto
                if (string.IsNullOrEmpty(primerProducto.ManualUsuarioPdf))
                {
                    // Este es un PDF básico válido en base64 que dice "Hello World! This is a test PDF manual"
                    primerProducto.ManualUsuarioPdf = "JVBERi0xLjQKJeLjz9MKNCAwIG9iago8PAovVHlwZSAvQ2F0YWxvZwovUGFnZXMgMiAwIFIKPj4KZW5kb2JqCjIgMCBvYmoKPDwKL1R5cGUgL1BhZ2VzCi9LaWRzIFsgMSAwIFIgXQovQ291bnQgMQo+PgplbmRvYmoKMSAwIG9iago8PAovVHlwZSAvUGFnZQovUGFyZW50IDIgMCBSCi9NZWRpYUJveCBbIDAgMCA2MTIgNzkyIF0KL0NvbnRlbnRzIDMgMCBSCj4+CmVuZG9iagozIDAgb2JqCjw8Ci9MZW5ndGggMTA4Cj4+CnN0cmVhbQpCVApxCjIgMCAwIDIgMzAwIDY1MCBjbQpCVAovRjEgMTIgVGYKKEhlbGxvIFdvcmxkISBUaGlzIGlzIGEgdGVzdCBQREYgbWFudWFsKSBUagpFVApRCmVuZHN0cmVhbQplbmRvYmoKeHJlZgowIDQKMDAwMDAwMDAwMCA2NTUzNSBmIAowMDAwMDAwMDM5IDAwMDAwIG4gCjAwMDAwMDAwOTMgMDAwMDAgbiAKMDAwMDAwMDE0NyAwMDAwMCBuIAp0cmFpbGVyCjw8Ci9TaXplIDQKL1Jvb3QgNCAwIFIKPj4Kc3RhcnR4cmVmCjMxMAolJUVPRgo=";
                    Console.WriteLine($"Manual PDF añadido al producto: {primerProducto.Nombre}");
                }

                await context.SaveChangesAsync();
                Console.WriteLine("✅ Datos de prueba creados exitosamente!");

                // Mostrar los datos creados
                var comprasCreadas = await context.ProductoComprado
                    .Where(pc => pc.UserId == user.Id)
                    .Include(pc => pc.Producto)
                    .Select(pc => new
                    {
                        pc.Id,
                        pc.FechaCompra,
                        ProductoNombre = pc.Producto.Nombre,
                        TieneManual = !string.IsNullOrEmpty(pc.Producto.ManualUsuarioPdf)
                    })
                    .ToListAsync();

                Console.WriteLine($"\n📋 Compras registradas para {user.Email}:");
                foreach (var compra in comprasCreadas)
                {
                    Console.WriteLine($"- ID: {compra.Id}, Producto: {compra.ProductoNombre}, Fecha: {compra.FechaCompra:dd/MM/yyyy}, Manual: {(compra.TieneManual ? "Sí" : "No")}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creando datos de prueba: {ex.Message}");
                throw;
            }
        }
    }
}
