using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _063_RecetaSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "recetas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_recetas_SucursalId",
                table: "recetas",
                column: "SucursalId");

            // ── Backfill: recetas clínicas heredan la sucursal de su ConsultaClinica;
            // las cargadas a mano (sin ConsultaClinicaId) no tienen forma real de saberlo
            // retroactivamente → Casa Central, mismo criterio que el resto de los backfills
            // de SucursalId en migraciones anteriores.
            migrationBuilder.Sql("""
                UPDATE "recetas" r
                SET "SucursalId" = cc."SucursalId"
                FROM "consultas_clinicas" cc
                WHERE r."ConsultaClinicaId" = cc."Id";

                UPDATE "recetas"
                SET "SucursalId" = (SELECT "Id" FROM sucursales WHERE "Codigo" = 'CC')
                WHERE "SucursalId" = 0;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_recetas_sucursales_SucursalId",
                table: "recetas",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_recetas_sucursales_SucursalId",
                table: "recetas");

            migrationBuilder.DropIndex(
                name: "IX_recetas_SucursalId",
                table: "recetas");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "recetas");
        }
    }
}
