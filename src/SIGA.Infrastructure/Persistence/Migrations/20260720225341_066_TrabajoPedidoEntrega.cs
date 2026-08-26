using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _066_TrabajoPedidoEntrega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntregadoPorId",
                table: "trabajos_pedido",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaEntrega",
                table: "trabajos_pedido",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetiradoPor",
                table: "trabajos_pedido",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_trabajos_pedido_EntregadoPorId",
                table: "trabajos_pedido",
                column: "EntregadoPorId");

            migrationBuilder.AddForeignKey(
                name: "FK_trabajos_pedido_users_EntregadoPorId",
                table: "trabajos_pedido",
                column: "EntregadoPorId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_trabajos_pedido_users_EntregadoPorId",
                table: "trabajos_pedido");

            migrationBuilder.DropIndex(
                name: "IX_trabajos_pedido_EntregadoPorId",
                table: "trabajos_pedido");

            migrationBuilder.DropColumn(
                name: "EntregadoPorId",
                table: "trabajos_pedido");

            migrationBuilder.DropColumn(
                name: "FechaEntrega",
                table: "trabajos_pedido");

            migrationBuilder.DropColumn(
                name: "RetiradoPor",
                table: "trabajos_pedido");
        }
    }
}
