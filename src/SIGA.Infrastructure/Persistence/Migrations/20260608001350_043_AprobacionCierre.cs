using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _043_AprobacionCierre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AprobadoPorId",
                table: "sesiones_caja",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAprobacion",
                table: "sesiones_caja",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRechazo",
                table: "sesiones_caja",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_caja_AprobadoPorId",
                table: "sesiones_caja",
                column: "AprobadoPorId");

            migrationBuilder.AddForeignKey(
                name: "FK_sesiones_caja_users_AprobadoPorId",
                table: "sesiones_caja",
                column: "AprobadoPorId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sesiones_caja_users_AprobadoPorId",
                table: "sesiones_caja");

            migrationBuilder.DropIndex(
                name: "IX_sesiones_caja_AprobadoPorId",
                table: "sesiones_caja");

            migrationBuilder.DropColumn(
                name: "AprobadoPorId",
                table: "sesiones_caja");

            migrationBuilder.DropColumn(
                name: "FechaAprobacion",
                table: "sesiones_caja");

            migrationBuilder.DropColumn(
                name: "MotivoRechazo",
                table: "sesiones_caja");
        }
    }
}
