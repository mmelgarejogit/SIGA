using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _038_TrabajoPedidoRecetaNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_trabajos_pedido_recetas_RecetaId",
                table: "trabajos_pedido");

            migrationBuilder.AlterColumn<int>(
                name: "RecetaId",
                table: "trabajos_pedido",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_trabajos_pedido_recetas_RecetaId",
                table: "trabajos_pedido",
                column: "RecetaId",
                principalTable: "recetas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_trabajos_pedido_recetas_RecetaId",
                table: "trabajos_pedido");

            migrationBuilder.AlterColumn<int>(
                name: "RecetaId",
                table: "trabajos_pedido",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_trabajos_pedido_recetas_RecetaId",
                table: "trabajos_pedido",
                column: "RecetaId",
                principalTable: "recetas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
