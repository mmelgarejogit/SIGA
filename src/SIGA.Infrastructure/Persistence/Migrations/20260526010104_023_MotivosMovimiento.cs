using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _023_MotivosMovimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MotivoMovimientoId",
                table: "MovimientosStock",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "motivos_movimiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_motivos_movimiento", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_MotivoMovimientoId",
                table: "MovimientosStock",
                column: "MotivoMovimientoId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosStock_motivos_movimiento_MotivoMovimientoId",
                table: "MovimientosStock",
                column: "MotivoMovimientoId",
                principalTable: "motivos_movimiento",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosStock_motivos_movimiento_MotivoMovimientoId",
                table: "MovimientosStock");

            migrationBuilder.DropTable(
                name: "motivos_movimiento");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosStock_MotivoMovimientoId",
                table: "MovimientosStock");

            migrationBuilder.DropColumn(
                name: "MotivoMovimientoId",
                table: "MovimientosStock");
        }
    }
}
