using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _024_MovimientoStockEstados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AprobadoPorNombre",
                table: "MovimientosStock",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreadoPorId",
                table: "MovimientosStock",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreadoPorNombre",
                table: "MovimientosStock",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "MovimientosStock",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAprobacion",
                table: "MovimientosStock",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaMovimiento",
                table: "MovimientosStock",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ObservacionesAprobacion",
                table: "MovimientosStock",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AprobadoPorNombre",
                table: "MovimientosStock");

            migrationBuilder.DropColumn(
                name: "CreadoPorId",
                table: "MovimientosStock");

            migrationBuilder.DropColumn(
                name: "CreadoPorNombre",
                table: "MovimientosStock");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "MovimientosStock");

            migrationBuilder.DropColumn(
                name: "FechaAprobacion",
                table: "MovimientosStock");

            migrationBuilder.DropColumn(
                name: "FechaMovimiento",
                table: "MovimientosStock");

            migrationBuilder.DropColumn(
                name: "ObservacionesAprobacion",
                table: "MovimientosStock");
        }
    }
}
