using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _012_ClinicaSoftDeleteAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "consultas_clinicas",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "consultas_clinicas");
        }
    }
}
