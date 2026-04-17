using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _005_RenameDNItoCI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DNI",
                table: "persons",
                newName: "CI");

            migrationBuilder.RenameIndex(
                name: "IX_persons_DNI",
                table: "persons",
                newName: "IX_persons_CI");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CI",
                table: "persons",
                newName: "DNI");

            migrationBuilder.RenameIndex(
                name: "IX_persons_CI",
                table: "persons",
                newName: "IX_persons_DNI");
        }
    }
}
