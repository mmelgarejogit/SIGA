using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _033_DevolucionesVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "devoluciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SolicitadoPorId = table.Column<int>(type: "integer", nullable: false),
                    ConfirmadoPorId = table.Column<int>(type: "integer", nullable: true),
                    ObservacionesRevision = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FechaRevision = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devoluciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_devoluciones_users_ConfirmadoPorId",
                        column: x => x.ConfirmadoPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_devoluciones_users_SolicitadoPorId",
                        column: x => x.SolicitadoPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_devoluciones_ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "devolucion_lineas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DevolucionId = table.Column<int>(type: "integer", nullable: false),
                    ProductoDevueltoId = table.Column<int>(type: "integer", nullable: false),
                    CantidadDevuelta = table.Column<int>(type: "integer", nullable: false),
                    ProductoNuevoId = table.Column<int>(type: "integer", nullable: true),
                    CantidadNueva = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devolucion_lineas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_devolucion_lineas_Productos_ProductoDevueltoId",
                        column: x => x.ProductoDevueltoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_devolucion_lineas_Productos_ProductoNuevoId",
                        column: x => x.ProductoNuevoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_devolucion_lineas_devoluciones_DevolucionId",
                        column: x => x.DevolucionId,
                        principalTable: "devoluciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_devolucion_lineas_DevolucionId",
                table: "devolucion_lineas",
                column: "DevolucionId");

            migrationBuilder.CreateIndex(
                name: "IX_devolucion_lineas_ProductoDevueltoId",
                table: "devolucion_lineas",
                column: "ProductoDevueltoId");

            migrationBuilder.CreateIndex(
                name: "IX_devolucion_lineas_ProductoNuevoId",
                table: "devolucion_lineas",
                column: "ProductoNuevoId");

            migrationBuilder.CreateIndex(
                name: "IX_devoluciones_ConfirmadoPorId",
                table: "devoluciones",
                column: "ConfirmadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_devoluciones_SolicitadoPorId",
                table: "devoluciones",
                column: "SolicitadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_devoluciones_VentaId",
                table: "devoluciones",
                column: "VentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "devolucion_lineas");

            migrationBuilder.DropTable(
                name: "devoluciones");
        }
    }
}
