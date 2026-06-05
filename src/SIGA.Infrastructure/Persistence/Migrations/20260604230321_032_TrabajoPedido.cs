using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _032_TrabajoPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsLaboratorio",
                table: "Proveedores",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "trabajos_pedido",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    RecetaId = table.Column<int>(type: "integer", nullable: false),
                    TipoLente = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tratamientos = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ArmazonProductoId = table.Column<int>(type: "integer", nullable: true),
                    LaboratorioProveedorId = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaEnvio = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaRecepcion = table.Column<DateOnly>(type: "date", nullable: true),
                    Observacion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trabajos_pedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trabajos_pedido_Productos_ArmazonProductoId",
                        column: x => x.ArmazonProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_trabajos_pedido_Proveedores_LaboratorioProveedorId",
                        column: x => x.LaboratorioProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trabajos_pedido_recetas_RecetaId",
                        column: x => x.RecetaId,
                        principalTable: "recetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trabajos_pedido_ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trabajos_pedido_ArmazonProductoId",
                table: "trabajos_pedido",
                column: "ArmazonProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_trabajos_pedido_LaboratorioProveedorId",
                table: "trabajos_pedido",
                column: "LaboratorioProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_trabajos_pedido_RecetaId",
                table: "trabajos_pedido",
                column: "RecetaId");

            migrationBuilder.CreateIndex(
                name: "IX_trabajos_pedido_VentaId",
                table: "trabajos_pedido",
                column: "VentaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trabajos_pedido");

            migrationBuilder.DropColumn(
                name: "EsLaboratorio",
                table: "Proveedores");
        }
    }
}
