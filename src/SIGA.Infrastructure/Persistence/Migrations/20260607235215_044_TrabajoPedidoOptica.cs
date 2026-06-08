using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _044_TrabajoPedidoOptica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TipoLenteId",
                table: "trabajos_pedido",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "LaboratorioProveedorId",
                table: "trabajos_pedido",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "ArmazonDelCliente",
                table: "trabajos_pedido",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_trabajos_pedido_Productos_CristalProductoId",
                table: "trabajos_pedido");

            migrationBuilder.DropIndex(
                name: "IX_trabajos_pedido_CristalProductoId",
                table: "trabajos_pedido");

            migrationBuilder.DropColumn(
                name: "ArmazonDelCliente",
                table: "trabajos_pedido");

            migrationBuilder.DropColumn(
                name: "CristalProductoId",
                table: "trabajos_pedido");

            migrationBuilder.AlterColumn<int>(
                name: "TipoLenteId",
                table: "trabajos_pedido",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LaboratorioProveedorId",
                table: "trabajos_pedido",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
