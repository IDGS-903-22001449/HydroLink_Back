using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HydroLink.Migrations
{
    /// <inheritdoc />
    public partial class FixPrecisionWarnings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Cantidad",
                table: "CotizacionDetalle",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Cantidad",
                table: "CotizacionDetalle",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);
        }
    }
}
