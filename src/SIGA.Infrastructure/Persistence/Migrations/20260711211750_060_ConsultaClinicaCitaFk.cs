using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _060_ConsultaClinicaCitaFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_consultas_clinicas_CitaId",
                table: "consultas_clinicas",
                column: "CitaId");

            migrationBuilder.AddForeignKey(
                name: "FK_consultas_clinicas_turnos_CitaId",
                table: "consultas_clinicas",
                column: "CitaId",
                principalTable: "turnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_consultas_clinicas_turnos_CitaId",
                table: "consultas_clinicas");

            migrationBuilder.DropIndex(
                name: "IX_consultas_clinicas_CitaId",
                table: "consultas_clinicas");
        }
    }
}
