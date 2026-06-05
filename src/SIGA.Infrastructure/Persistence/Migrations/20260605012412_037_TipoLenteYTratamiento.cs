using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _037_TipoLenteYTratamiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoLente",
                table: "trabajos_pedido");

            migrationBuilder.DropColumn(
                name: "Tratamientos",
                table: "trabajos_pedido");

            migrationBuilder.AddColumn<int>(
                name: "TipoLenteId",
                table: "trabajos_pedido",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "tipos_lente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_lente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tratamientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tratamientos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trabajos_pedido_tratamientos",
                columns: table => new
                {
                    TrabajosPedidoId = table.Column<int>(type: "integer", nullable: false),
                    TratamientosId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trabajos_pedido_tratamientos", x => new { x.TrabajosPedidoId, x.TratamientosId });
                    table.ForeignKey(
                        name: "FK_trabajos_pedido_tratamientos_trabajos_pedido_TrabajosPedido~",
                        column: x => x.TrabajosPedidoId,
                        principalTable: "trabajos_pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_trabajos_pedido_tratamientos_tratamientos_TratamientosId",
                        column: x => x.TratamientosId,
                        principalTable: "tratamientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trabajos_pedido_TipoLenteId",
                table: "trabajos_pedido",
                column: "TipoLenteId");

            migrationBuilder.CreateIndex(
                name: "IX_tipos_lente_Nombre",
                table: "tipos_lente",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trabajos_pedido_tratamientos_TratamientosId",
                table: "trabajos_pedido_tratamientos",
                column: "TratamientosId");

            migrationBuilder.CreateIndex(
                name: "IX_tratamientos_Nombre",
                table: "tratamientos",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_trabajos_pedido_tipos_lente_TipoLenteId",
                table: "trabajos_pedido",
                column: "TipoLenteId",
                principalTable: "tipos_lente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_trabajos_pedido_tipos_lente_TipoLenteId",
                table: "trabajos_pedido");

            migrationBuilder.DropTable(
                name: "tipos_lente");

            migrationBuilder.DropTable(
                name: "trabajos_pedido_tratamientos");

            migrationBuilder.DropTable(
                name: "tratamientos");

            migrationBuilder.DropIndex(
                name: "IX_trabajos_pedido_TipoLenteId",
                table: "trabajos_pedido");

            migrationBuilder.DropColumn(
                name: "TipoLenteId",
                table: "trabajos_pedido");

            migrationBuilder.AddColumn<string>(
                name: "TipoLente",
                table: "trabajos_pedido",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tratamientos",
                table: "trabajos_pedido",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
