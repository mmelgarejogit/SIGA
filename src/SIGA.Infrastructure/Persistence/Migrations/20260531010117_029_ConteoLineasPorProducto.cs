using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _029_ConteoLineasPorProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConteoInventarioLineas_StockLotes_LoteId",
                table: "ConteoInventarioLineas");

            migrationBuilder.DropIndex(
                name: "IX_ConteoInventarioLineas_LoteId",
                table: "ConteoInventarioLineas");

            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                table: "ConteoInventarioLineas");

            migrationBuilder.DropColumn(
                name: "LoteId",
                table: "ConteoInventarioLineas");

            migrationBuilder.DropColumn(
                name: "LoteNumero",
                table: "ConteoInventarioLineas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FechaVencimiento",
                table: "ConteoInventarioLineas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoteId",
                table: "ConteoInventarioLineas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LoteNumero",
                table: "ConteoInventarioLineas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ConteoInventarioLineas_LoteId",
                table: "ConteoInventarioLineas",
                column: "LoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConteoInventarioLineas_StockLotes_LoteId",
                table: "ConteoInventarioLineas",
                column: "LoteId",
                principalTable: "StockLotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
