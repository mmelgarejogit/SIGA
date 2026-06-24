using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _027_ConteoInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConteosInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreadoPorId = table.Column<int>(type: "integer", nullable: false),
                    CreadoPorNombre = table.Column<string>(type: "text", nullable: false),
                    FechaConteo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    AprobadoPorNombre = table.Column<string>(type: "text", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ObservacionesAprobacion = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConteosInventario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConteoInventarioLineas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConteoId = table.Column<int>(type: "integer", nullable: false),
                    LoteId = table.Column<int>(type: "integer", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    LoteNumero = table.Column<string>(type: "text", nullable: false),
                    FechaVencimiento = table.Column<string>(type: "text", nullable: true),
                    CantidadSistema = table.Column<int>(type: "integer", nullable: false),
                    CantidadFisica = table.Column<int>(type: "integer", nullable: false),
                    Diferencia = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConteoInventarioLineas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConteoInventarioLineas_ConteosInventario_ConteoId",
                        column: x => x.ConteoId,
                        principalTable: "ConteosInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConteoInventarioLineas_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConteoInventarioLineas_StockLotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "StockLotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConteoInventarioLineas_ConteoId",
                table: "ConteoInventarioLineas",
                column: "ConteoId");

            migrationBuilder.CreateIndex(
                name: "IX_ConteoInventarioLineas_LoteId",
                table: "ConteoInventarioLineas",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ConteoInventarioLineas_ProductoId",
                table: "ConteoInventarioLineas",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConteoInventarioLineas");

            migrationBuilder.DropTable(
                name: "ConteosInventario");
        }
    }
}
