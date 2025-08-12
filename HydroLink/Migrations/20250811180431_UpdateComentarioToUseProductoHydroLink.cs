using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HydroLink.Migrations
{
    /// <inheritdoc />
    public partial class UpdateComentarioToUseProductoHydroLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentario_Persona_UsuarioId",
                table: "Comentario");

            migrationBuilder.DropForeignKey(
                name: "FK_Comentario_Producto_ProductoId",
                table: "Comentario");

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioId",
                table: "Comentario",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ProductoId",
                table: "Comentario",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ProductoHydroLinkId",
                table: "Comentario",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId1",
                table: "Comentario",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comentario_ProductoHydroLinkId_UsuarioId",
                table: "Comentario",
                columns: new[] { "ProductoHydroLinkId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comentario_UsuarioId1",
                table: "Comentario",
                column: "UsuarioId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentario_AspNetUsers_UsuarioId",
                table: "Comentario",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comentario_Persona_UsuarioId1",
                table: "Comentario",
                column: "UsuarioId1",
                principalTable: "Persona",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentario_ProductoHydroLink_ProductoHydroLinkId",
                table: "Comentario",
                column: "ProductoHydroLinkId",
                principalTable: "ProductoHydroLink",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comentario_Producto_ProductoId",
                table: "Comentario",
                column: "ProductoId",
                principalTable: "Producto",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentario_AspNetUsers_UsuarioId",
                table: "Comentario");

            migrationBuilder.DropForeignKey(
                name: "FK_Comentario_Persona_UsuarioId1",
                table: "Comentario");

            migrationBuilder.DropForeignKey(
                name: "FK_Comentario_ProductoHydroLink_ProductoHydroLinkId",
                table: "Comentario");

            migrationBuilder.DropForeignKey(
                name: "FK_Comentario_Producto_ProductoId",
                table: "Comentario");

            migrationBuilder.DropIndex(
                name: "IX_Comentario_ProductoHydroLinkId_UsuarioId",
                table: "Comentario");

            migrationBuilder.DropIndex(
                name: "IX_Comentario_UsuarioId1",
                table: "Comentario");

            migrationBuilder.DropColumn(
                name: "ProductoHydroLinkId",
                table: "Comentario");

            migrationBuilder.DropColumn(
                name: "UsuarioId1",
                table: "Comentario");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Comentario",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "ProductoId",
                table: "Comentario",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comentario_Persona_UsuarioId",
                table: "Comentario",
                column: "UsuarioId",
                principalTable: "Persona",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comentario_Producto_ProductoId",
                table: "Comentario",
                column: "ProductoId",
                principalTable: "Producto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
