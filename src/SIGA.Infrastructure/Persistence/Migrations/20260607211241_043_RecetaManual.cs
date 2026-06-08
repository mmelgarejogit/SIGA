using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _043_RecetaManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ConsultaClinicaId",
                table: "recetas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "PersonId",
                table: "recetas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_recetas_PersonId",
                table: "recetas",
                column: "PersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_recetas_persons_PersonId",
                table: "recetas",
                column: "PersonId",
                principalTable: "persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Backfill: las recetas clínicas existentes heredan la persona desde su consulta→paciente.
            migrationBuilder.Sql(@"
                UPDATE recetas r
                SET ""PersonId"" = pt.""PersonId""
                FROM consultas_clinicas c
                JOIN patients pt ON pt.""Id"" = c.""PatientId""
                WHERE r.""ConsultaClinicaId"" = c.""Id"" AND r.""PersonId"" IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_recetas_persons_PersonId",
                table: "recetas");

            migrationBuilder.DropIndex(
                name: "IX_recetas_PersonId",
                table: "recetas");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "recetas");

            migrationBuilder.AlterColumn<int>(
                name: "ConsultaClinicaId",
                table: "recetas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
