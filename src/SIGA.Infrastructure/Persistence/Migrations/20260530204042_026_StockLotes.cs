using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _026_StockLotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockLotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    RecepcionItemId = table.Column<int>(type: "integer", nullable: false),
                    Lote = table.Column<string>(type: "text", nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    CantidadInicial = table.Column<int>(type: "integer", nullable: false),
                    FechaIngreso = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockLotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockLotes_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockLotes_RecepcionesMercaderiaItems_RecepcionItemId",
                        column: x => x.RecepcionItemId,
                        principalTable: "RecepcionesMercaderiaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockLotes_ProductoId",
                table: "StockLotes",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_StockLotes_RecepcionItemId",
                table: "StockLotes",
                column: "RecepcionItemId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockLotes");
        }
    }
}
