using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _026_EgresoAprobacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAprobacion",
                table: "egresos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRechazo",
                table: "egresos",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NroComprobante",
                table: "egresos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaAprobacion",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "MotivoRechazo",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "NroComprobante",
                table: "egresos");
        }
    }
}
