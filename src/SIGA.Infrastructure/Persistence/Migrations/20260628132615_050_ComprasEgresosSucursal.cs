using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _050_ComprasEgresosSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "RecepcionesMercaderia",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "PedidosProveedor",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "egresos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesMercaderia_SucursalId",
                table: "RecepcionesMercaderia",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosProveedor_SucursalId",
                table: "PedidosProveedor",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_egresos_SucursalId",
                table: "egresos",
                column: "SucursalId");

            // ── Backfill: filas existentes (SucursalId = 0 por defaultValue) → Casa Central ──
            migrationBuilder.Sql("""
                UPDATE "PedidosProveedor"      SET "SucursalId" = (SELECT "Id" FROM sucursales WHERE "Codigo" = 'CC') WHERE "SucursalId" = 0;
                UPDATE "RecepcionesMercaderia" SET "SucursalId" = (SELECT "Id" FROM sucursales WHERE "Codigo" = 'CC') WHERE "SucursalId" = 0;
                UPDATE "egresos"               SET "SucursalId" = (SELECT "Id" FROM sucursales WHERE "Codigo" = 'CC') WHERE "SucursalId" = 0;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_egresos_sucursales_SucursalId",
                table: "egresos",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PedidosProveedor_sucursales_SucursalId",
                table: "PedidosProveedor",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecepcionesMercaderia_sucursales_SucursalId",
                table: "RecepcionesMercaderia",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_egresos_sucursales_SucursalId",
                table: "egresos");

            migrationBuilder.DropForeignKey(
                name: "FK_PedidosProveedor_sucursales_SucursalId",
                table: "PedidosProveedor");

            migrationBuilder.DropForeignKey(
                name: "FK_RecepcionesMercaderia_sucursales_SucursalId",
                table: "RecepcionesMercaderia");

            migrationBuilder.DropIndex(
                name: "IX_RecepcionesMercaderia_SucursalId",
                table: "RecepcionesMercaderia");

            migrationBuilder.DropIndex(
                name: "IX_PedidosProveedor_SucursalId",
                table: "PedidosProveedor");

            migrationBuilder.DropIndex(
                name: "IX_egresos_SucursalId",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "RecepcionesMercaderia");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "PedidosProveedor");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "egresos");
        }
    }
}
