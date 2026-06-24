using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Timbrados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimbradoId",
                table: "facturas_venta",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "timbrados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroTimbrado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Establecimiento = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PuntoExpedicion = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    UltimoNumero = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NumeroDesde = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    NumeroHasta = table.Column<int>(type: "integer", nullable: true),
                    FechaInicioVigencia = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFinVigencia = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timbrados", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_facturas_venta_TimbradoId",
                table: "facturas_venta",
                column: "TimbradoId");

            migrationBuilder.CreateIndex(
                name: "IX_timbrados_NumeroTimbrado_Establecimiento_PuntoExpedicion",
                table: "timbrados",
                columns: new[] { "NumeroTimbrado", "Establecimiento", "PuntoExpedicion" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_facturas_venta_timbrados_TimbradoId",
                table: "facturas_venta",
                column: "TimbradoId",
                principalTable: "timbrados",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_facturas_venta_timbrados_TimbradoId",
                table: "facturas_venta");

            migrationBuilder.DropTable(
                name: "timbrados");

            migrationBuilder.DropIndex(
                name: "IX_facturas_venta_TimbradoId",
                table: "facturas_venta");

            migrationBuilder.DropColumn(
                name: "TimbradoId",
                table: "facturas_venta");
        }
    }
}
