using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _053_PacientesGlobales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Los pacientes son globales (sin sucursal): elijen una al reservar el turno.
            // Corrige cualquier paciente que un backfill anterior haya asignado a una sucursal.
            migrationBuilder.Sql("""
                UPDATE users
                SET "SucursalId" = NULL
                WHERE "Id" IN (
                    SELECT ur."UserId"
                    FROM user_roles ur
                    JOIN roles r ON r."Id" = ur."RoleId"
                    WHERE r."Type" = 'patient'
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
