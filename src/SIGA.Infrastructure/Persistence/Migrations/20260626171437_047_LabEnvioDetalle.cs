using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _047_LabEnvioDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaEstimadaEntrega",
                table: "trabajos_pedido",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MedioEnvio",
                table: "trabajos_pedido",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaEstimadaEntrega",
                table: "trabajos_pedido");

            migrationBuilder.DropColumn(
                name: "MedioEnvio",
                table: "trabajos_pedido");
        }
    }
}
