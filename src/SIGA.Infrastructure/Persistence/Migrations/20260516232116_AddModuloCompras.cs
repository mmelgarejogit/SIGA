using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloCompras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidosProveedor_estados_config_EstadoConfigId",
                table: "PedidosProveedor");

            migrationBuilder.DropIndex(
                name: "IX_PedidosProveedor_EstadoConfigId",
                table: "PedidosProveedor");

            migrationBuilder.DropColumn(
                name: "EstadoConfigId",
                table: "PedidosProveedor");

            migrationBuilder.AddColumn<int>(
                name: "CantidadRecibida",
                table: "PedidosProveedorItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "PedidosProveedor",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DevolucionesProveedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoProveedorId = table.Column<int>(type: "integer", nullable: false),
                    PedidoProveedorItemId = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    Motivo = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevolucionesProveedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevolucionesProveedor_PedidosProveedorItems_PedidoProveedor~",
                        column: x => x.PedidoProveedorItemId,
                        principalTable: "PedidosProveedorItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DevolucionesProveedor_PedidosProveedor_PedidoProveedorId",
                        column: x => x.PedidoProveedorId,
                        principalTable: "PedidosProveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesProveedor_PedidoProveedorId",
                table: "DevolucionesProveedor",
                column: "PedidoProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesProveedor_PedidoProveedorItemId",
                table: "DevolucionesProveedor",
                column: "PedidoProveedorItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevolucionesProveedor");

            migrationBuilder.DropColumn(
                name: "CantidadRecibida",
                table: "PedidosProveedorItems");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "PedidosProveedor");

            migrationBuilder.AddColumn<int>(
                name: "EstadoConfigId",
                table: "PedidosProveedor",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PedidosProveedor_EstadoConfigId",
                table: "PedidosProveedor",
                column: "EstadoConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_PedidosProveedor_estados_config_EstadoConfigId",
                table: "PedidosProveedor",
                column: "EstadoConfigId",
                principalTable: "estados_config",
                principalColumn: "Id");
        }
    }
}
