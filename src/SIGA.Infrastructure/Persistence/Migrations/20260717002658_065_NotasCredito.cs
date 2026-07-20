using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _065_NotasCredito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "timbrados",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "notas_credito",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DevolucionId = table.Column<int>(type: "integer", nullable: false),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    FacturaVentaId = table.Column<int>(type: "integer", nullable: false),
                    NumeroNotaCredito = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Timbrado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Establecimiento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MontoExento = table.Column<decimal>(type: "numeric(18,0)", nullable: false),
                    MontoGravado5 = table.Column<decimal>(type: "numeric(18,0)", nullable: false),
                    MontoGravado10 = table.Column<decimal>(type: "numeric(18,0)", nullable: false),
                    FechaEmision = table.Column<DateOnly>(type: "date", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EmitidoPorId = table.Column<int>(type: "integer", nullable: false),
                    TimbradoId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notas_credito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notas_credito_devoluciones_DevolucionId",
                        column: x => x.DevolucionId,
                        principalTable: "devoluciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notas_credito_facturas_venta_FacturaVentaId",
                        column: x => x.FacturaVentaId,
                        principalTable: "facturas_venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notas_credito_timbrados_TimbradoId",
                        column: x => x.TimbradoId,
                        principalTable: "timbrados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notas_credito_users_EmitidoPorId",
                        column: x => x.EmitidoPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notas_credito_ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notas_credito_DevolucionId",
                table: "notas_credito",
                column: "DevolucionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notas_credito_EmitidoPorId",
                table: "notas_credito",
                column: "EmitidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_notas_credito_FacturaVentaId",
                table: "notas_credito",
                column: "FacturaVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_notas_credito_TimbradoId",
                table: "notas_credito",
                column: "TimbradoId");

            migrationBuilder.CreateIndex(
                name: "IX_notas_credito_VentaId",
                table: "notas_credito",
                column: "VentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notas_credito");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "timbrados");
        }
    }
}
