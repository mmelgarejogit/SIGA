using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _030_InventarioFisico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inventarios_fisicos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    Alcance = table.Column<string>(type: "text", nullable: false),
                    FiltroCategoriaId = table.Column<int>(type: "integer", nullable: true),
                    FechaInicioConteo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IniciadoPorId = table.Column<int>(type: "integer", nullable: false),
                    EjecutadoPorId = table.Column<int>(type: "integer", nullable: true),
                    AprobadoPorId = table.Column<int>(type: "integer", nullable: true),
                    Observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventarios_fisicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inventarios_fisicos_categorias_producto_FiltroCategoriaId",
                        column: x => x.FiltroCategoriaId,
                        principalTable: "categorias_producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventarios_fisicos_sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventarios_fisicos_users_AprobadoPorId",
                        column: x => x.AprobadoPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventarios_fisicos_users_EjecutadoPorId",
                        column: x => x.EjecutadoPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventarios_fisicos_users_IniciadoPorId",
                        column: x => x.IniciadoPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inventario_fisico_lineas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    InventarioFisicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoVarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CantidadSistema = table.Column<int>(type: "integer", nullable: false),
                    CantidadContada = table.Column<int>(type: "integer", nullable: true),
                    Diferencia = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventario_fisico_lineas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inventario_fisico_lineas_inventarios_fisicos_InventarioFisi~",
                        column: x => x.InventarioFisicoId,
                        principalTable: "inventarios_fisicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_inventario_fisico_lineas_producto_variantes_ProductoVariant~",
                        column: x => x.ProductoVarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inventario_fisico_lineas_InventarioFisicoId",
                table: "inventario_fisico_lineas",
                column: "InventarioFisicoId");

            migrationBuilder.CreateIndex(
                name: "IX_inventario_fisico_lineas_ProductoVarianteId",
                table: "inventario_fisico_lineas",
                column: "ProductoVarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_inventarios_fisicos_AprobadoPorId",
                table: "inventarios_fisicos",
                column: "AprobadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_inventarios_fisicos_EjecutadoPorId",
                table: "inventarios_fisicos",
                column: "EjecutadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_inventarios_fisicos_FiltroCategoriaId",
                table: "inventarios_fisicos",
                column: "FiltroCategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_inventarios_fisicos_IniciadoPorId",
                table: "inventarios_fisicos",
                column: "IniciadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_inventarios_fisicos_SucursalId_Estado",
                table: "inventarios_fisicos",
                columns: new[] { "SucursalId", "Estado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventario_fisico_lineas");

            migrationBuilder.DropTable(
                name: "inventarios_fisicos");
        }
    }
}
