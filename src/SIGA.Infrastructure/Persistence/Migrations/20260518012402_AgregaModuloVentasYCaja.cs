using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregaModuloVentasYCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "servicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Precio = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroComprobante = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    RecetaId = table.Column<int>(type: "integer", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    CondicionVenta = table.Column<int>(type: "integer", nullable: false),
                    FechaVenta = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ventas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ventas_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ventas_recetas_RecetaId",
                        column: x => x.RecetaId,
                        principalTable: "recetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "cobros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MetodoPago = table.Column<int>(type: "integer", nullable: false),
                    FechaCobro = table.Column<DateOnly>(type: "date", nullable: false),
                    Referencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Anulado = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cobros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cobros_ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "facturas_venta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    NumeroFactura = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Timbrado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Establecimiento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MontoExento = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontoGravado5 = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontoGravado10 = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FechaEmision = table.Column<DateOnly>(type: "date", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facturas_venta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_facturas_venta_ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movimientos_caja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Concepto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    MetodoPago = table.Column<int>(type: "integer", nullable: false),
                    VentaId = table.Column<int>(type: "integer", nullable: true),
                    EgresoId = table.Column<int>(type: "integer", nullable: true),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Referencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos_caja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_movimientos_caja_egresos_EgresoId",
                        column: x => x.EgresoId,
                        principalTable: "egresos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_movimientos_caja_ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "venta_lineas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: true),
                    ServicioId = table.Column<int>(type: "integer", nullable: true),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CategoriaFiscal = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venta_lineas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_venta_lineas_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_venta_lineas_servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_venta_lineas_ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cobros_VentaId",
                table: "cobros",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_facturas_venta_VentaId",
                table: "facturas_venta",
                column: "VentaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_caja_EgresoId",
                table: "movimientos_caja",
                column: "EgresoId");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_caja_VentaId",
                table: "movimientos_caja",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_venta_lineas_ProductoId",
                table: "venta_lineas",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_venta_lineas_ServicioId",
                table: "venta_lineas",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_venta_lineas_VentaId",
                table: "venta_lineas",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_ventas_PatientId",
                table: "ventas",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ventas_RecetaId",
                table: "ventas",
                column: "RecetaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cobros");

            migrationBuilder.DropTable(
                name: "facturas_venta");

            migrationBuilder.DropTable(
                name: "movimientos_caja");

            migrationBuilder.DropTable(
                name: "venta_lineas");

            migrationBuilder.DropTable(
                name: "servicios");

            migrationBuilder.DropTable(
                name: "ventas");
        }
    }
}
