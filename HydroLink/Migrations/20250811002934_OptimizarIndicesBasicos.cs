using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HydroLink.Migrations
{
    /// <inheritdoc />
    public partial class OptimizarIndicesBasicos : Migration
    {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Índices básicos para mejorar rendimiento (sin campos problemáticos)
        
        // ProductoHydroLink - Filtro por Activo (usado en casi todas las consultas)
        migrationBuilder.CreateIndex(
            name: "IX_ProductoHydroLink_Activo",
            table: "ProductoHydroLink",
            column: "Activo");
            
        // ProductoHydroLink - Ordenamiento por FechaCreacion
        migrationBuilder.CreateIndex(
            name: "IX_ProductoHydroLink_FechaCreacion",
            table: "ProductoHydroLink",
            column: "FechaCreacion");
            
        // Componente - Filtro por Activo
        migrationBuilder.CreateIndex(
            name: "IX_Componente_Activo",
            table: "Componente",
            column: "Activo");
            
        // Venta - Ordenamiento por Fecha
        migrationBuilder.CreateIndex(
            name: "IX_Venta_Fecha",
            table: "Venta",
            column: "Fecha");
    }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar índices creados
            migrationBuilder.DropIndex(
                name: "IX_ProductoHydroLink_Activo",
                table: "ProductoHydroLink");
                
            migrationBuilder.DropIndex(
                name: "IX_ProductoHydroLink_FechaCreacion",
                table: "ProductoHydroLink");
                
            migrationBuilder.DropIndex(
                name: "IX_Componente_Activo",
                table: "Componente");
                
            migrationBuilder.DropIndex(
                name: "IX_Venta_Fecha",
                table: "Venta");
        }
    }
}
