using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _039_TrabajosPedidoSegmentado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AprobadoPorId",
                table: "trabajos_pedido",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionAprobacion",
                table: "trabajos_pedido",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "facturas_laboratorio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrabajoPedidoId = table.Column<int>(type: "integer", nullable: false),
                    NumeroFactura = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Timbrado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FechaEmision = table.Column<DateOnly>(type: "date", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EmitidoPorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facturas_laboratorio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_facturas_laboratorio_trabajos_pedido_TrabajoPedidoId",
                        column: x => x.TrabajoPedidoId,
                        principalTable: "trabajos_pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_facturas_laboratorio_users_EmitidoPorId",
                        column: x => x.EmitidoPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trabajos_pedido_AprobadoPorId",
                table: "trabajos_pedido",
                column: "AprobadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_facturas_laboratorio_EmitidoPorId",
                table: "facturas_laboratorio",
                column: "EmitidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_facturas_laboratorio_TrabajoPedidoId",
                table: "facturas_laboratorio",
                column: "TrabajoPedidoId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_trabajos_pedido_users_AprobadoPorId",
                table: "trabajos_pedido",
                column: "AprobadoPorId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_trabajos_pedido_users_AprobadoPorId",
                table: "trabajos_pedido");

            migrationBuilder.DropTable(
                name: "facturas_laboratorio");

            migrationBuilder.DropIndex(
                name: "IX_trabajos_pedido_AprobadoPorId",
                table: "trabajos_pedido");

            migrationBuilder.DropColumn(
                name: "AprobadoPorId",
                table: "trabajos_pedido");

            migrationBuilder.DropColumn(
                name: "ObservacionAprobacion",
                table: "trabajos_pedido");
        }
    }
}
