using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _028_StockModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosStock_Productos_ProductoId",
                table: "MovimientosStock");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosStock_motivos_movimiento_MotivoMovimientoId",
                table: "MovimientosStock");

            migrationBuilder.DropForeignKey(
                name: "FK_PedidosProveedorItems_Productos_ProductoId",
                table: "PedidosProveedorItems");

            migrationBuilder.DropForeignKey(
                name: "FK_venta_lineas_Productos_ProductoId",
                table: "venta_lineas");

            migrationBuilder.DropIndex(
                name: "IX_venta_lineas_ProductoId",
                table: "venta_lineas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MovimientosStock",
                table: "MovimientosStock");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "venta_lineas");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrecioCosto",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrecioVenta",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "StockActual",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "StockMinimo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Talle",
                table: "Productos");

            migrationBuilder.RenameTable(
                name: "MovimientosStock",
                newName: "MovimientoStock");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientosStock_ProductoId",
                table: "MovimientoStock",
                newName: "IX_MovimientoStock_ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientosStock_MotivoMovimientoId",
                table: "MovimientoStock",
                newName: "IX_MovimientoStock_MotivoMovimientoId");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductoVarianteId",
                table: "venta_lineas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductoId",
                table: "PedidosProveedorItems",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductoVarianteId",
                table: "PedidosProveedorItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductoVarianteId",
                table: "FacturaCompraItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MovimientoStock",
                table: "MovimientoStock",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "producto_variantes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Talle = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PrecioCosto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ImagenUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto_variantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_producto_variantes_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sucursales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Direccion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sucursales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tipos_ajuste",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Impacto = table.Column<string>(type: "text", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_ajuste", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "parametros_stock",
                columns: table => new
                {
                    ProductoVarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockMinimo = table.Column<int>(type: "integer", nullable: false),
                    StockMaximo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parametros_stock", x => new { x.ProductoVarianteId, x.SucursalId });
                    table.ForeignKey(
                        name: "FK_parametros_stock_producto_variantes_ProductoVarianteId",
                        column: x => x.ProductoVarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_parametros_stock_sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transferencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SucursalOrigenId = table.Column<Guid>(type: "uuid", nullable: false),
                    SucursalDestinoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    SolicitadoPorId = table.Column<int>(type: "integer", nullable: false),
                    AprobadoPorId = table.Column<int>(type: "integer", nullable: true),
                    Observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MotivoRechazo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transferencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transferencias_sucursales_SucursalDestinoId",
                        column: x => x.SucursalDestinoId,
                        principalTable: "sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transferencias_sucursales_SucursalOrigenId",
                        column: x => x.SucursalOrigenId,
                        principalTable: "sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transferencias_users_AprobadoPorId",
                        column: x => x.AprobadoPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transferencias_users_SolicitadoPorId",
                        column: x => x.SolicitadoPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ajustes_manual",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoAjusteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoVarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    CreadoPorId = table.Column<int>(type: "integer", nullable: false),
                    AprobadoPorId = table.Column<int>(type: "integer", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ObservacionResolucion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ajustes_manual", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ajustes_manual_producto_variantes_ProductoVarianteId",
                        column: x => x.ProductoVarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ajustes_manual_sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ajustes_manual_tipos_ajuste_TipoAjusteId",
                        column: x => x.TipoAjusteId,
                        principalTable: "tipos_ajuste",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ajustes_manual_users_AprobadoPorId",
                        column: x => x.AprobadoPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ajustes_manual_users_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimientos_inventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ProductoVarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    OrigenTipo = table.Column<string>(type: "text", nullable: false),
                    ReferenciaId = table.Column<Guid>(type: "uuid", nullable: true),
                    TipoAjusteId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos_inventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_movimientos_inventario_producto_variantes_ProductoVarianteId",
                        column: x => x.ProductoVarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimientos_inventario_sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimientos_inventario_tipos_ajuste_TipoAjusteId",
                        column: x => x.TipoAjusteId,
                        principalTable: "tipos_ajuste",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimientos_inventario_users_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transferencia_lineas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    TransferenciaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoVarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transferencia_lineas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transferencia_lineas_producto_variantes_ProductoVarianteId",
                        column: x => x.ProductoVarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transferencia_lineas_transferencias_TransferenciaId",
                        column: x => x.TransferenciaId,
                        principalTable: "transferencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_venta_lineas_ProductoVarianteId",
                table: "venta_lineas",
                column: "ProductoVarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosProveedorItems_ProductoVarianteId",
                table: "PedidosProveedorItems",
                column: "ProductoVarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaCompraItems_ProductoVarianteId",
                table: "FacturaCompraItems",
                column: "ProductoVarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_ajustes_manual_AprobadoPorId",
                table: "ajustes_manual",
                column: "AprobadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ajustes_manual_CreadoPorId",
                table: "ajustes_manual",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ajustes_manual_ProductoVarianteId",
                table: "ajustes_manual",
                column: "ProductoVarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_ajustes_manual_SucursalId",
                table: "ajustes_manual",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_ajustes_manual_TipoAjusteId",
                table: "ajustes_manual",
                column: "TipoAjusteId");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_inventario_ProductoVarianteId_SucursalId_Fecha",
                table: "movimientos_inventario",
                columns: new[] { "ProductoVarianteId", "SucursalId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_inventario_SucursalId",
                table: "movimientos_inventario",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_inventario_TipoAjusteId",
                table: "movimientos_inventario",
                column: "TipoAjusteId");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_inventario_UsuarioId",
                table: "movimientos_inventario",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_parametros_stock_SucursalId",
                table: "parametros_stock",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_producto_variantes_ProductoId",
                table: "producto_variantes",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_sucursales_Codigo",
                table: "sucursales",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transferencia_lineas_ProductoVarianteId",
                table: "transferencia_lineas",
                column: "ProductoVarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_transferencia_lineas_TransferenciaId",
                table: "transferencia_lineas",
                column: "TransferenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_transferencias_AprobadoPorId",
                table: "transferencias",
                column: "AprobadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_transferencias_SolicitadoPorId",
                table: "transferencias",
                column: "SolicitadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_transferencias_SucursalDestinoId",
                table: "transferencias",
                column: "SucursalDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_transferencias_SucursalOrigenId",
                table: "transferencias",
                column: "SucursalOrigenId");

            migrationBuilder.AddForeignKey(
                name: "FK_FacturaCompraItems_producto_variantes_ProductoVarianteId",
                table: "FacturaCompraItems",
                column: "ProductoVarianteId",
                principalTable: "producto_variantes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoStock_Productos_ProductoId",
                table: "MovimientoStock",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoStock_motivos_movimiento_MotivoMovimientoId",
                table: "MovimientoStock",
                column: "MotivoMovimientoId",
                principalTable: "motivos_movimiento",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PedidosProveedorItems_Productos_ProductoId",
                table: "PedidosProveedorItems",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PedidosProveedorItems_producto_variantes_ProductoVarianteId",
                table: "PedidosProveedorItems",
                column: "ProductoVarianteId",
                principalTable: "producto_variantes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_venta_lineas_producto_variantes_ProductoVarianteId",
                table: "venta_lineas",
                column: "ProductoVarianteId",
                principalTable: "producto_variantes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FacturaCompraItems_producto_variantes_ProductoVarianteId",
                table: "FacturaCompraItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoStock_Productos_ProductoId",
                table: "MovimientoStock");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoStock_motivos_movimiento_MotivoMovimientoId",
                table: "MovimientoStock");

            migrationBuilder.DropForeignKey(
                name: "FK_PedidosProveedorItems_Productos_ProductoId",
                table: "PedidosProveedorItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PedidosProveedorItems_producto_variantes_ProductoVarianteId",
                table: "PedidosProveedorItems");

            migrationBuilder.DropForeignKey(
                name: "FK_venta_lineas_producto_variantes_ProductoVarianteId",
                table: "venta_lineas");

            migrationBuilder.DropTable(
                name: "ajustes_manual");

            migrationBuilder.DropTable(
                name: "movimientos_inventario");

            migrationBuilder.DropTable(
                name: "parametros_stock");

            migrationBuilder.DropTable(
                name: "transferencia_lineas");

            migrationBuilder.DropTable(
                name: "tipos_ajuste");

            migrationBuilder.DropTable(
                name: "producto_variantes");

            migrationBuilder.DropTable(
                name: "transferencias");

            migrationBuilder.DropTable(
                name: "sucursales");

            migrationBuilder.DropIndex(
                name: "IX_venta_lineas_ProductoVarianteId",
                table: "venta_lineas");

            migrationBuilder.DropIndex(
                name: "IX_PedidosProveedorItems_ProductoVarianteId",
                table: "PedidosProveedorItems");

            migrationBuilder.DropIndex(
                name: "IX_FacturaCompraItems_ProductoVarianteId",
                table: "FacturaCompraItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MovimientoStock",
                table: "MovimientoStock");

            migrationBuilder.DropColumn(
                name: "ProductoVarianteId",
                table: "venta_lineas");

            migrationBuilder.DropColumn(
                name: "ProductoVarianteId",
                table: "PedidosProveedorItems");

            migrationBuilder.DropColumn(
                name: "ProductoVarianteId",
                table: "FacturaCompraItems");

            migrationBuilder.RenameTable(
                name: "MovimientoStock",
                newName: "MovimientosStock");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientoStock_ProductoId",
                table: "MovimientosStock",
                newName: "IX_MovimientosStock_ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientoStock_MotivoMovimientoId",
                table: "MovimientosStock",
                newName: "IX_MovimientosStock_MotivoMovimientoId");

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "venta_lineas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Productos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Productos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "Productos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioCosto",
                table: "Productos",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioVenta",
                table: "Productos",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "Productos",
                type: "text",
                nullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "Talle",
                table: "Productos",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductoId",
                table: "PedidosProveedorItems",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MovimientosStock",
                table: "MovimientosStock",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_venta_lineas_ProductoId",
                table: "venta_lineas",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosStock_Productos_ProductoId",
                table: "MovimientosStock",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosStock_motivos_movimiento_MotivoMovimientoId",
                table: "MovimientosStock",
                column: "MotivoMovimientoId",
                principalTable: "motivos_movimiento",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PedidosProveedorItems_Productos_ProductoId",
                table: "PedidosProveedorItems",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_venta_lineas_Productos_ProductoId",
                table: "venta_lineas",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
