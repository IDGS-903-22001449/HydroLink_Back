using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HydroLink.Migrations
{
    /// <inheritdoc />
    public partial class FixPersonaCotizaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cotizacion_Persona_ClienteId1",
                table: "Cotizacion");

            migrationBuilder.DropForeignKey(
                name: "FK_Cotizacion_ProductoHydroLink_ProductoId",
                table: "Cotizacion");

            migrationBuilder.DropForeignKey(
                name: "FK_Cotizacion_Venta_VentaId",
                table: "Cotizacion");

            migrationBuilder.DropIndex(
                name: "IX_Cotizacion_VentaId",
                table: "Cotizacion");

            migrationBuilder.RenameColumn(
                name: "ClienteId1",
                table: "Cotizacion",
                newName: "ProveedorId");

            migrationBuilder.RenameIndex(
                name: "IX_Cotizacion_ClienteId1",
                table: "Cotizacion",
                newName: "IX_Cotizacion_ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Venta_CotizacionId",
                table: "Venta",
                column: "CotizacionId",
                unique: true,
                filter: "[CotizacionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizacion_Persona_ProveedorId",
                table: "Cotizacion",
                column: "ProveedorId",
                principalTable: "Persona",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizacion_ProductoHydroLink_ProductoId",
                table: "Cotizacion",
                column: "ProductoId",
                principalTable: "ProductoHydroLink",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Venta_Cotizacion_CotizacionId",
                table: "Venta",
                column: "CotizacionId",
                principalTable: "Cotizacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cotizacion_Persona_ProveedorId",
                table: "Cotizacion");

            migrationBuilder.DropForeignKey(
                name: "FK_Cotizacion_ProductoHydroLink_ProductoId",
                table: "Cotizacion");

            migrationBuilder.DropForeignKey(
                name: "FK_Venta_Cotizacion_CotizacionId",
                table: "Venta");

            migrationBuilder.DropIndex(
                name: "IX_Venta_CotizacionId",
                table: "Venta");

            migrationBuilder.RenameColumn(
                name: "ProveedorId",
                table: "Cotizacion",
                newName: "ClienteId1");

            migrationBuilder.RenameIndex(
                name: "IX_Cotizacion_ProveedorId",
                table: "Cotizacion",
                newName: "IX_Cotizacion_ClienteId1");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_VentaId",
                table: "Cotizacion",
                column: "VentaId",
                unique: true,
                filter: "[VentaId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizacion_Persona_ClienteId1",
                table: "Cotizacion",
                column: "ClienteId1",
                principalTable: "Persona",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizacion_ProductoHydroLink_ProductoId",
                table: "Cotizacion",
                column: "ProductoId",
                principalTable: "ProductoHydroLink",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizacion_Venta_VentaId",
                table: "Cotizacion",
                column: "VentaId",
                principalTable: "Venta",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
