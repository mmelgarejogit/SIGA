using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _048_EgresoFacturaLaboratorio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "factura_laboratorio_id",
                table: "egresos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_egresos_factura_laboratorio_id",
                table: "egresos",
                column: "factura_laboratorio_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_egresos_facturas_laboratorio_factura_laboratorio_id",
                table: "egresos",
                column: "factura_laboratorio_id",
                principalTable: "facturas_laboratorio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_egresos_facturas_laboratorio_factura_laboratorio_id",
                table: "egresos");

            migrationBuilder.DropIndex(
                name: "IX_egresos_factura_laboratorio_id",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "factura_laboratorio_id",
                table: "egresos");
        }
    }
}
