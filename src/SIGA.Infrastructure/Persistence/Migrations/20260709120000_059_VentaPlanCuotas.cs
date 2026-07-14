using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _059_VentaPlanCuotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CantidadCuotas",
                table: "ventas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FrecuenciaCuotasDias",
                table: "ventas",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadCuotas",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "FrecuenciaCuotasDias",
                table: "ventas");
        }
    }
}
