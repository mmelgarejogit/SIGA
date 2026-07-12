using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _061_EnumsEstadoLibre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AlterColumn<int> con oldClrType(string) no sirve acá: Postgres no puede
            // castear implícitamente "Pendiente"::text a integer. Cada columna necesita
            // su propio USING con el mapeo string->ordinal (mismo orden que el enum C#).
            // Algunas de estas columnas tienen un DEFAULT string a nivel Postgres (ej.
            // MovimientosStock.Estado, migración 025) que tampoco castea automático —
            // se dropea antes de cada ALTER TYPE por las dudas (no-op si no hay default).
            migrationBuilder.Sql("""
                ALTER TABLE "transferencias_stock" ALTER COLUMN "Estado" DROP DEFAULT;
                ALTER TABLE "transferencias_stock" ALTER COLUMN "Estado" TYPE integer USING (
                    CASE "Estado"
                        WHEN 'Pendiente' THEN 0
                        WHEN 'Aceptada'  THEN 1
                        WHEN 'Rechazada' THEN 2
                    END);
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "notificaciones_internas" ALTER COLUMN "Tipo" DROP DEFAULT;
                ALTER TABLE "notificaciones_internas" ALTER COLUMN "Tipo" TYPE integer USING (
                    CASE "Tipo"
                        WHEN 'bajo_stock'              THEN 0
                        WHEN 'transferencia_pendiente'  THEN 1
                        WHEN 'pedido_lab_recibido'      THEN 2
                    END);
                """);

            // vw_stock_actual referencia MovimientosStock.Tipo/.Estado en su CASE/WHERE —
            // hay que tirarla antes de alterar esas dos columnas y recrearla después con
            // los ordinales nuevos (Postgres no permite alterar una columna usada por una vista).
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_stock_actual;");

            migrationBuilder.Sql("""
                ALTER TABLE "MovimientosStock" ALTER COLUMN "Tipo" DROP DEFAULT;
                ALTER TABLE "MovimientosStock" ALTER COLUMN "Tipo" TYPE integer USING (
                    CASE "Tipo"
                        WHEN 'Entrada' THEN 0
                        WHEN 'Salida'  THEN 1
                        WHEN 'Ajuste'  THEN 2
                    END);
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "MovimientosStock" ALTER COLUMN "Estado" DROP DEFAULT;
                ALTER TABLE "MovimientosStock" ALTER COLUMN "Estado" TYPE integer USING (
                    CASE "Estado"
                        WHEN 'Pendiente' THEN 0
                        WHEN 'Aprobado'  THEN 1
                        WHEN 'Rechazado' THEN 2
                    END);
                """);

            migrationBuilder.Sql("""
                CREATE VIEW vw_stock_actual AS
                SELECT
                    m."ProductoId" AS producto_id,
                    m."SucursalId" AS sucursal_id,
                    COALESCE(SUM(
                        CASE m."Tipo"
                            WHEN 0 THEN  m."Cantidad"
                            WHEN 1 THEN -m."Cantidad"
                            ELSE 0
                        END
                    ), 0) AS stock_actual
                FROM "MovimientosStock" m
                WHERE m."Estado" = 1
                GROUP BY m."ProductoId", m."SucursalId";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "motivos_movimiento" ALTER COLUMN "Tipo" DROP DEFAULT;
                ALTER TABLE "motivos_movimiento" ALTER COLUMN "Tipo" TYPE integer USING (
                    CASE "Tipo"
                        WHEN 'Entrada' THEN 0
                        WHEN 'Salida'  THEN 1
                        WHEN 'Ambos'   THEN 2
                    END);
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "ConteosInventario" ALTER COLUMN "Estado" DROP DEFAULT;
                ALTER TABLE "ConteosInventario" ALTER COLUMN "Estado" TYPE integer USING (
                    CASE "Estado"
                        WHEN 'Pendiente' THEN 0
                        WHEN 'Aprobado'  THEN 1
                        WHEN 'Rechazado' THEN 2
                    END);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "transferencias_stock" ALTER COLUMN "Estado" TYPE character varying(20) USING (
                    CASE "Estado" WHEN 0 THEN 'Pendiente' WHEN 1 THEN 'Aceptada' WHEN 2 THEN 'Rechazada' END);
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "notificaciones_internas" ALTER COLUMN "Tipo" TYPE character varying(50) USING (
                    CASE "Tipo" WHEN 0 THEN 'bajo_stock' WHEN 1 THEN 'transferencia_pendiente' WHEN 2 THEN 'pedido_lab_recibido' END);
                """);

            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_stock_actual;");

            migrationBuilder.Sql("""
                ALTER TABLE "MovimientosStock" ALTER COLUMN "Tipo" TYPE text USING (
                    CASE "Tipo" WHEN 0 THEN 'Entrada' WHEN 1 THEN 'Salida' WHEN 2 THEN 'Ajuste' END);
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "MovimientosStock" ALTER COLUMN "Estado" TYPE text USING (
                    CASE "Estado" WHEN 0 THEN 'Pendiente' WHEN 1 THEN 'Aprobado' WHEN 2 THEN 'Rechazado' END);
                """);

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

            migrationBuilder.Sql("""
                ALTER TABLE "motivos_movimiento" ALTER COLUMN "Tipo" TYPE character varying(20) USING (
                    CASE "Tipo" WHEN 0 THEN 'Entrada' WHEN 1 THEN 'Salida' WHEN 2 THEN 'Ambos' END);
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "ConteosInventario" ALTER COLUMN "Estado" TYPE text USING (
                    CASE "Estado" WHEN 0 THEN 'Pendiente' WHEN 1 THEN 'Aprobado' WHEN 2 THEN 'Rechazado' END);
                """);
        }
    }
}
