using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _022_ComprasRecepciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecepcionesMercaderia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoProveedorId = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionesMercaderia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecepcionesMercaderia_PedidosProveedor_PedidoProveedorId",
                        column: x => x.PedidoProveedorId,
                        principalTable: "PedidosProveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecepcionesMercaderiaItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecepcionId = table.Column<int>(type: "integer", nullable: false),
                    PedidoItemId = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionesMercaderiaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecepcionesMercaderiaItems_PedidosProveedorItems_PedidoItem~",
                        column: x => x.PedidoItemId,
                        principalTable: "PedidosProveedorItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecepcionesMercaderiaItems_RecepcionesMercaderia_RecepcionId",
                        column: x => x.RecepcionId,
                        principalTable: "RecepcionesMercaderia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesMercaderia_PedidoProveedorId",
                table: "RecepcionesMercaderia",
                column: "PedidoProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesMercaderiaItems_PedidoItemId",
                table: "RecepcionesMercaderiaItems",
                column: "PedidoItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesMercaderiaItems_RecepcionId",
                table: "RecepcionesMercaderiaItems",
                column: "RecepcionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecepcionesMercaderiaItems");

            migrationBuilder.DropTable(
                name: "RecepcionesMercaderia");
        }
    }
}
