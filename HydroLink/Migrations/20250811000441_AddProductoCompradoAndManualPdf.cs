using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HydroLink.Migrations
{
    /// <inheritdoc />
    public partial class AddProductoCompradoAndManualPdf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ManualUsuarioPdf",
                table: "ProductoHydroLink",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductoComprado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    FechaCompra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VentaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoComprado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductoComprado_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductoComprado_ProductoHydroLink_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "ProductoHydroLink",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductoComprado_Venta_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductoComprado_ProductoId",
                table: "ProductoComprado",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoComprado_UserId_ProductoId",
                table: "ProductoComprado",
                columns: new[] { "UserId", "ProductoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductoComprado_VentaId",
                table: "ProductoComprado",
                column: "VentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductoComprado");

            migrationBuilder.DropColumn(
                name: "ManualUsuarioPdf",
                table: "ProductoHydroLink");
        }
    }
}
