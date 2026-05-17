using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloEgresos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias_gasto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias_gasto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "egresos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,0)", nullable: false),
                    Concepto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FechaEmision = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaPago = table.Column<DateOnly>(type: "date", nullable: true),
                    MetodoPago = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NroFactura = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PedidoProveedorId = table.Column<int>(type: "integer", nullable: true),
                    ProveedorId = table.Column<int>(type: "integer", nullable: true),
                    CategoriaGastoId = table.Column<int>(type: "integer", nullable: true),
                    ProfessionalId = table.Column<int>(type: "integer", nullable: true),
                    Periodo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_egresos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_egresos_PedidosProveedor_PedidoProveedorId",
                        column: x => x.PedidoProveedorId,
                        principalTable: "PedidosProveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_egresos_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_egresos_categorias_gasto_CategoriaGastoId",
                        column: x => x.CategoriaGastoId,
                        principalTable: "categorias_gasto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_egresos_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_egresos_CategoriaGastoId",
                table: "egresos",
                column: "CategoriaGastoId");

            migrationBuilder.CreateIndex(
                name: "IX_egresos_PedidoProveedorId",
                table: "egresos",
                column: "PedidoProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_egresos_ProfessionalId",
                table: "egresos",
                column: "ProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_egresos_ProveedorId",
                table: "egresos",
                column: "ProveedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "egresos");

            migrationBuilder.DropTable(
                name: "categorias_gasto");
        }
    }
}
