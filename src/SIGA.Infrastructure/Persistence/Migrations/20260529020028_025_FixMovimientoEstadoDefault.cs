using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _025_FixMovimientoEstadoDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rows created before the Estado column was added got defaultValue: "" from EF.
            // Backfill them to "Pendiente" so the approval workflow applies correctly.
            migrationBuilder.Sql("""
                UPDATE "MovimientosStock"
                SET "Estado" = 'Pendiente'
                WHERE "Estado" = '';
                """);

            // Also set a proper SQL-level default so future ADD COLUMN operations don't repeat this.
            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "MovimientosStock",
                type: "text",
                nullable: false,
                defaultValue: "Pendiente",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "MovimientosStock",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Pendiente");
        }
    }
}
