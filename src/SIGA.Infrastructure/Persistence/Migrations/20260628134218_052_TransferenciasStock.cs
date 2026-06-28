using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _052_TransferenciasStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transferencias_stock",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SucursalOrigenId = table.Column<int>(type: "integer", nullable: false),
                    SucursalDestinoId = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreadoPorId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreadoPorNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RecibidoPorNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transferencias_stock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transferencias_stock_sucursales_SucursalDestinoId",
                        column: x => x.SucursalDestinoId,
                        principalTable: "sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transferencias_stock_sucursales_SucursalOrigenId",
                        column: x => x.SucursalOrigenId,
                        principalTable: "sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transferencias_stock_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransferenciaStockId = table.Column<int>(type: "integer", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transferencias_stock_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transferencias_stock_items_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transferencias_stock_items_transferencias_stock_Transferenc~",
                        column: x => x.TransferenciaStockId,
                        principalTable: "transferencias_stock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transferencias_stock_SucursalDestinoId",
                table: "transferencias_stock",
                column: "SucursalDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_transferencias_stock_SucursalOrigenId",
                table: "transferencias_stock",
                column: "SucursalOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_transferencias_stock_items_ProductoId",
                table: "transferencias_stock_items",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_transferencias_stock_items_TransferenciaStockId",
                table: "transferencias_stock_items",
                column: "TransferenciaStockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transferencias_stock_items");

            migrationBuilder.DropTable(
                name: "transferencias_stock");
        }
    }
}
