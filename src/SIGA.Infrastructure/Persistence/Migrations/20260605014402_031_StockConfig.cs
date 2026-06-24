using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _031_StockConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Crear tabla de configuración de stock
            migrationBuilder.CreateTable(
                name: "productos_stock_config",
                columns: table => new
                {
                    producto_id  = table.Column<int>(type: "integer", nullable: false),
                    stock_minimo = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    stock_maximo = table.Column<int>(type: "integer", nullable: true),
                    updated_at   = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos_stock_config", x => x.producto_id);
                    table.ForeignKey(
                        name: "FK_productos_stock_config_Productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 2. Migrar datos de stock_minimo / stock_maximo antes de quitar las columnas
            migrationBuilder.Sql("""
                INSERT INTO productos_stock_config (producto_id, stock_minimo, stock_maximo, updated_at)
                SELECT "Id", "StockMinimo", "StockMaximo", now()
                FROM "Productos";
                """);

            // 3. Quitar columnas de Productos
            migrationBuilder.DropColumn(name: "StockActual", table: "Productos");
            migrationBuilder.DropColumn(name: "StockMinimo", table: "Productos");
            migrationBuilder.DropColumn(name: "StockMaximo", table: "Productos");

            // 4. Crear vista vw_stock_actual
            migrationBuilder.Sql("""
                CREATE VIEW vw_stock_actual AS
                SELECT
                    m."ProductoId"   AS producto_id,
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_stock_actual;");

            // Restaurar columnas en Productos
            migrationBuilder.AddColumn<int>(
                name: "StockActual",
                table: "Productos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockMinimo",
                table: "Productos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockMaximo",
                table: "Productos",
                type: "integer",
                nullable: true);

            // Restaurar datos
            migrationBuilder.Sql("""
                UPDATE "Productos" p
                SET "StockMinimo" = c.stock_minimo,
                    "StockMaximo" = c.stock_maximo
                FROM productos_stock_config c
                WHERE p."Id" = c.producto_id;
                """);

            // Recalcular StockActual desde movimientos aprobados
            migrationBuilder.Sql("""
                UPDATE "Productos" p
                SET "StockActual" = COALESCE((
                    SELECT SUM(CASE "Tipo" WHEN 'Entrada' THEN "Cantidad" WHEN 'Salida' THEN -"Cantidad" ELSE 0 END)
                    FROM "MovimientosStock"
                    WHERE "ProductoId" = p."Id" AND "Estado" = 'Aprobado'
                ), 0);
                """);

            migrationBuilder.DropTable(name: "productos_stock_config");
        }
    }
}
