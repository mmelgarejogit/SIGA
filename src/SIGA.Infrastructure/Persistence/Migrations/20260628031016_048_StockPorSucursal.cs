using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _048_StockPorSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "StockLotes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "MovimientosStock",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "ConteosInventario",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StockLotes_SucursalId",
                table: "StockLotes",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_SucursalId",
                table: "MovimientosStock",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_ConteosInventario_SucursalId",
                table: "ConteosInventario",
                column: "SucursalId");

            // ── Backfill: las filas existentes (SucursalId = 0 por defaultValue) → Casa Central ──
            migrationBuilder.Sql("""
                UPDATE "MovimientosStock"  SET "SucursalId" = (SELECT "Id" FROM sucursales WHERE "Codigo" = 'CC') WHERE "SucursalId" = 0;
                UPDATE "StockLotes"        SET "SucursalId" = (SELECT "Id" FROM sucursales WHERE "Codigo" = 'CC') WHERE "SucursalId" = 0;
                UPDATE "ConteosInventario" SET "SucursalId" = (SELECT "Id" FROM sucursales WHERE "Codigo" = 'CC') WHERE "SucursalId" = 0;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_ConteosInventario_sucursales_SucursalId",
                table: "ConteosInventario",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosStock_sucursales_SucursalId",
                table: "MovimientosStock",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockLotes_sucursales_SucursalId",
                table: "StockLotes",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ── Reconstruir la vista de stock agregando la dimensión sucursal ──
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_stock_actual;");
            migrationBuilder.Sql("""
                CREATE VIEW vw_stock_actual AS
                SELECT
                    m."ProductoId" AS producto_id,
                    m."SucursalId" AS sucursal_id,
                    COALESCE(SUM(
                        CASE m."Tipo"
                            WHEN 'Entrada' THEN  m."Cantidad"
                            WHEN 'Salida'  THEN -m."Cantidad"
                            ELSE 0
                        END
                    ), 0) AS stock_actual
                FROM "MovimientosStock" m
                WHERE m."Estado" = 'Aprobado'
                GROUP BY m."ProductoId", m."SucursalId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restaurar la vista original (sin sucursal) antes de quitar la columna de la que depende
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_stock_actual;");
            migrationBuilder.Sql("""
                CREATE VIEW vw_stock_actual AS
                SELECT
                    m."ProductoId" AS producto_id,
                    COALESCE(SUM(
                        CASE m."Tipo"
                            WHEN 'Entrada' THEN  m."Cantidad"
                            WHEN 'Salida'  THEN -m."Cantidad"
                            ELSE 0
                        END
                    ), 0) AS stock_actual
                FROM "MovimientosStock" m
                WHERE m."Estado" = 'Aprobado'
                GROUP BY m."ProductoId";
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_ConteosInventario_sucursales_SucursalId",
                table: "ConteosInventario");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosStock_sucursales_SucursalId",
                table: "MovimientosStock");

            migrationBuilder.DropForeignKey(
                name: "FK_StockLotes_sucursales_SucursalId",
                table: "StockLotes");

            migrationBuilder.DropIndex(
                name: "IX_StockLotes_SucursalId",
                table: "StockLotes");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosStock_SucursalId",
                table: "MovimientosStock");

            migrationBuilder.DropIndex(
                name: "IX_ConteosInventario_SucursalId",
                table: "ConteosInventario");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "StockLotes");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "MovimientosStock");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "ConteosInventario");
        }
    }
}
