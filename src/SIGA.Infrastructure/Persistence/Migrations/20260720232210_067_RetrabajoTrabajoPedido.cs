using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _067_RetrabajoTrabajoPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "retrabajos_trabajo_pedido",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrabajoPedidoId = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Motivo = table.Column<int>(type: "integer", nullable: false),
                    Responsable = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RegistradoPorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_retrabajos_trabajo_pedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_retrabajos_trabajo_pedido_trabajos_pedido_TrabajoPedidoId",
                        column: x => x.TrabajoPedidoId,
                        principalTable: "trabajos_pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_retrabajos_trabajo_pedido_users_RegistradoPorId",
                        column: x => x.RegistradoPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_retrabajos_trabajo_pedido_RegistradoPorId",
                table: "retrabajos_trabajo_pedido",
                column: "RegistradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_retrabajos_trabajo_pedido_TrabajoPedidoId",
                table: "retrabajos_trabajo_pedido",
                column: "TrabajoPedidoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "retrabajos_trabajo_pedido");
        }
    }
}
