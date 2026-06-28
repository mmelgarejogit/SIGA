using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _049_VentasCajaTimbradoSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "ventas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "timbrados",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "sesiones_caja",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "movimientos_caja",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ventas_SucursalId",
                table: "ventas",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_timbrados_SucursalId",
                table: "timbrados",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_caja_SucursalId",
                table: "sesiones_caja",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_caja_SucursalId",
                table: "movimientos_caja",
                column: "SucursalId");

            // ── Backfill: filas existentes (SucursalId = 0 por defaultValue) → Casa Central ──
            migrationBuilder.Sql("""
                UPDATE "ventas"           SET "SucursalId" = (SELECT "Id" FROM sucursales WHERE "Codigo" = 'CC') WHERE "SucursalId" = 0;
                UPDATE "timbrados"        SET "SucursalId" = (SELECT "Id" FROM sucursales WHERE "Codigo" = 'CC') WHERE "SucursalId" = 0;
                UPDATE "sesiones_caja"    SET "SucursalId" = (SELECT "Id" FROM sucursales WHERE "Codigo" = 'CC') WHERE "SucursalId" = 0;
                UPDATE "movimientos_caja" SET "SucursalId" = (SELECT "Id" FROM sucursales WHERE "Codigo" = 'CC') WHERE "SucursalId" = 0;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_movimientos_caja_sucursales_SucursalId",
                table: "movimientos_caja",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_sesiones_caja_sucursales_SucursalId",
                table: "sesiones_caja",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_timbrados_sucursales_SucursalId",
                table: "timbrados",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ventas_sucursales_SucursalId",
                table: "ventas",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_movimientos_caja_sucursales_SucursalId",
                table: "movimientos_caja");

            migrationBuilder.DropForeignKey(
                name: "FK_sesiones_caja_sucursales_SucursalId",
                table: "sesiones_caja");

            migrationBuilder.DropForeignKey(
                name: "FK_timbrados_sucursales_SucursalId",
                table: "timbrados");

            migrationBuilder.DropForeignKey(
                name: "FK_ventas_sucursales_SucursalId",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "IX_ventas_SucursalId",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "IX_timbrados_SucursalId",
                table: "timbrados");

            migrationBuilder.DropIndex(
                name: "IX_sesiones_caja_SucursalId",
                table: "sesiones_caja");

            migrationBuilder.DropIndex(
                name: "IX_movimientos_caja_SucursalId",
                table: "movimientos_caja");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "timbrados");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "sesiones_caja");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "movimientos_caja");
        }
    }
}
