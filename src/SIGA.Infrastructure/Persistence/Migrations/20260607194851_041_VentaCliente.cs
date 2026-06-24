using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _041_VentaCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Reset del módulo de ventas/presupuestos: al pasar de Paciente a Cliente
            // se descartan las ventas existentes (data de dev). Se borra en orden de
            // dependencias de FK; no se toca movimientos_stock (no tiene FK a venta).
            migrationBuilder.Sql(@"
                DELETE FROM movimientos_caja WHERE ""VentaId"" IS NOT NULL;
                DELETE FROM devolucion_lineas;
                DELETE FROM devoluciones;
                DELETE FROM cobro_lineas;
                DELETE FROM cobros;
                DELETE FROM facturas_laboratorio;
                DELETE FROM trabajos_pedido_tratamientos;
                DELETE FROM trabajos_pedido;
                DELETE FROM facturas_venta;
                DELETE FROM comprobantes;
                DELETE FROM venta_lineas;
                DELETE FROM ventas;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_ventas_patients_PatientId",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "IX_ventas_PatientId",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "ventas");

            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "ventas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ventas_ClienteId",
                table: "ventas",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_ventas_clientes_ClienteId",
                table: "ventas",
                column: "ClienteId",
                principalTable: "clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ventas_clientes_ClienteId",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "IX_ventas_ClienteId",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "ventas");

            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "ventas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ventas_PatientId",
                table: "ventas",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ventas_patients_PatientId",
                table: "ventas",
                column: "PatientId",
                principalTable: "patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
