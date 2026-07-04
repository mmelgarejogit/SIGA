using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _051_TurnosClinicaSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_horarios_profesional_ProfessionalId_DiaSemana",
                table: "horarios_profesional");

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "turnos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "horarios_profesional",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "consultas_clinicas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_turnos_SucursalId",
                table: "turnos",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_horarios_profesional_ProfessionalId_SucursalId_DiaSemana",
                table: "horarios_profesional",
                columns: new[] { "ProfessionalId", "SucursalId", "DiaSemana" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_horarios_profesional_SucursalId",
                table: "horarios_profesional",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_consultas_clinicas_SucursalId",
                table: "consultas_clinicas",
                column: "SucursalId");

            // ── Backfill: filas existentes (SucursalId = 0 por defaultValue) → Casa Central ──
            migrationBuilder.Sql("""
                UPDATE "turnos"              SET "SucursalId" = (SELECT "Id" FROM sucursales WHERE "Codigo" = 'CC') WHERE "SucursalId" = 0;
                UPDATE "horarios_profesional" SET "SucursalId" = (SELECT "Id" FROM sucursales WHERE "Codigo" = 'CC') WHERE "SucursalId" = 0;
                UPDATE "consultas_clinicas"  SET "SucursalId" = (SELECT "Id" FROM sucursales WHERE "Codigo" = 'CC') WHERE "SucursalId" = 0;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_consultas_clinicas_sucursales_SucursalId",
                table: "consultas_clinicas",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_horarios_profesional_sucursales_SucursalId",
                table: "horarios_profesional",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_turnos_sucursales_SucursalId",
                table: "turnos",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_consultas_clinicas_sucursales_SucursalId",
                table: "consultas_clinicas");

            migrationBuilder.DropForeignKey(
                name: "FK_horarios_profesional_sucursales_SucursalId",
                table: "horarios_profesional");

            migrationBuilder.DropForeignKey(
                name: "FK_turnos_sucursales_SucursalId",
                table: "turnos");

            migrationBuilder.DropIndex(
                name: "IX_turnos_SucursalId",
                table: "turnos");

            migrationBuilder.DropIndex(
                name: "IX_horarios_profesional_ProfessionalId_SucursalId_DiaSemana",
                table: "horarios_profesional");

            migrationBuilder.DropIndex(
                name: "IX_horarios_profesional_SucursalId",
                table: "horarios_profesional");

            migrationBuilder.DropIndex(
                name: "IX_consultas_clinicas_SucursalId",
                table: "consultas_clinicas");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "turnos");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "horarios_profesional");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "consultas_clinicas");

            migrationBuilder.CreateIndex(
                name: "IX_horarios_profesional_ProfessionalId_DiaSemana",
                table: "horarios_profesional",
                columns: new[] { "ProfessionalId", "DiaSemana" },
                unique: true);
        }
    }
}
