using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _054_OperadorVentaEgreso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VendedorId",
                table: "ventas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegistradoPorId",
                table: "egresos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ventas_VendedorId",
                table: "ventas",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_egresos_RegistradoPorId",
                table: "egresos",
                column: "RegistradoPorId");

            migrationBuilder.AddForeignKey(
                name: "FK_egresos_users_RegistradoPorId",
                table: "egresos",
                column: "RegistradoPorId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ventas_users_VendedorId",
                table: "ventas",
                column: "VendedorId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_egresos_users_RegistradoPorId",
                table: "egresos");

            migrationBuilder.DropForeignKey(
                name: "FK_ventas_users_VendedorId",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "IX_ventas_VendedorId",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "IX_egresos_RegistradoPorId",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "VendedorId",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "RegistradoPorId",
                table: "egresos");
        }
    }
}
