using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _046_ModeloLentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_trabajos_pedido_Productos_CristalProductoId",
                table: "trabajos_pedido");

            migrationBuilder.DropIndex(
                name: "IX_trabajos_pedido_CristalProductoId",
                table: "trabajos_pedido");

            migrationBuilder.DropColumn(
                name: "CristalProductoId",
                table: "trabajos_pedido");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioBase",
                table: "tipos_lente",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecioBase",
                table: "tipos_lente");

            migrationBuilder.AddColumn<int>(
                name: "CristalProductoId",
                table: "trabajos_pedido",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_trabajos_pedido_CristalProductoId",
                table: "trabajos_pedido",
                column: "CristalProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_trabajos_pedido_Productos_CristalProductoId",
                table: "trabajos_pedido",
                column: "CristalProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
