using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _041_SesionCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegistradoPorId",
                table: "movimientos_caja",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SesionCajaId",
                table: "movimientos_caja",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "sesiones_caja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    MontoInicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AbiertaPorId = table.Column<int>(type: "integer", nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CerradaPorId = table.Column<int>(type: "integer", nullable: true),
                    FechaCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EfectivoContado = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    EfectivoEsperado = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Diferencia = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ObservacionCierre = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesiones_caja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sesiones_caja_users_AbiertaPorId",
                        column: x => x.AbiertaPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sesiones_caja_users_CerradaPorId",
                        column: x => x.CerradaPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_caja_RegistradoPorId",
                table: "movimientos_caja",
                column: "RegistradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_caja_SesionCajaId",
                table: "movimientos_caja",
                column: "SesionCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_caja_AbiertaPorId",
                table: "sesiones_caja",
                column: "AbiertaPorId");

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_caja_CerradaPorId",
                table: "sesiones_caja",
                column: "CerradaPorId");

            migrationBuilder.AddForeignKey(
                name: "FK_movimientos_caja_sesiones_caja_SesionCajaId",
                table: "movimientos_caja",
                column: "SesionCajaId",
                principalTable: "sesiones_caja",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_movimientos_caja_users_RegistradoPorId",
                table: "movimientos_caja",
                column: "RegistradoPorId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_movimientos_caja_sesiones_caja_SesionCajaId",
                table: "movimientos_caja");

            migrationBuilder.DropForeignKey(
                name: "FK_movimientos_caja_users_RegistradoPorId",
                table: "movimientos_caja");

            migrationBuilder.DropTable(
                name: "sesiones_caja");

            migrationBuilder.DropIndex(
                name: "IX_movimientos_caja_RegistradoPorId",
                table: "movimientos_caja");

            migrationBuilder.DropIndex(
                name: "IX_movimientos_caja_SesionCajaId",
                table: "movimientos_caja");

            migrationBuilder.DropColumn(
                name: "RegistradoPorId",
                table: "movimientos_caja");

            migrationBuilder.DropColumn(
                name: "SesionCajaId",
                table: "movimientos_caja");
        }
    }
}
