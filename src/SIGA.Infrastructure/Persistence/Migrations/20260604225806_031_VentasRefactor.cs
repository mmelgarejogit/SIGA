using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _031_VentasRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Referencia",
                table: "cobros");

            migrationBuilder.RenameColumn(
                name: "FechaVencimiento",
                table: "ventas",
                newName: "FechaConfirmacion");

            migrationBuilder.RenameColumn(
                name: "Monto",
                table: "cobros",
                newName: "MontoTotal");

            migrationBuilder.RenameColumn(
                name: "MetodoPago",
                table: "cobros",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "FechaCobro",
                table: "cobros",
                newName: "Fecha");

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaComprobante",
                table: "ventas",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "ventas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RegistradoPorId",
                table: "cobros",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "cobro_lineas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CobroId = table.Column<int>(type: "integer", nullable: false),
                    MetodoPago = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cobro_lineas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cobro_lineas_cobros_CobroId",
                        column: x => x.CobroId,
                        principalTable: "cobros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comprobantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    MotivoAnulacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EmitidoPorId = table.Column<int>(type: "integer", nullable: false),
                    AnuladoPorId = table.Column<int>(type: "integer", nullable: true),
                    FechaEmision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaAnulacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comprobantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comprobantes_users_AnuladoPorId",
                        column: x => x.AnuladoPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comprobantes_users_EmitidoPorId",
                        column: x => x.EmitidoPorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comprobantes_ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ── Data migration ────────────────────────────────────────────────────────

            // Ventas: convertir valores de EstadoVenta al nuevo enum
            // Old: Abierta=0, Confirmada=1, PendienteDePago=2, Pagada=3, Cobrada=4, Anulada=5, AnulacionPendiente=6
            // New: Borrador=1, Confirmada=2, EnProceso=3, ListaParaCobrar=4, ComprobanteEmitido=5, Cancelada=6
            migrationBuilder.Sql(@"
                UPDATE ventas SET ""Estado"" = CASE
                    WHEN ""Estado"" = 0 THEN 1
                    WHEN ""Estado"" = 1 THEN 2
                    WHEN ""Estado"" = 2 THEN 4
                    WHEN ""Estado"" = 3 THEN 5
                    WHEN ""Estado"" = 4 THEN 5
                    WHEN ""Estado"" = 5 THEN 6
                    WHEN ""Estado"" = 6 THEN 2
                    ELSE 1
                END;
                UPDATE ventas SET ""Tipo"" = 1;
            ");

            // Cobros: la columna Tipo fue renombrada desde MetodoPago (contiene valores 0-3)
            // Migrar RegistradoPorId al primer usuario disponible
            migrationBuilder.Sql(@"
                UPDATE cobros
                SET ""RegistradoPorId"" = (SELECT ""Id"" FROM users ORDER BY ""Id"" LIMIT 1)
                WHERE ""RegistradoPorId"" = 0;
            ");

            // Crear cobro_lineas para cada cobro existente preservando el MetodoPago original
            migrationBuilder.Sql(@"
                INSERT INTO cobro_lineas (""CobroId"", ""MetodoPago"", ""Monto"")
                SELECT ""Id"", ""Tipo"", ""MontoTotal""
                FROM cobros;
            ");

            // Ahora sí poner el tipo semántico correcto (todos eran cuotas simples antes)
            migrationBuilder.Sql(@"UPDATE cobros SET ""Tipo"" = 2;");

            // ── Indexes ───────────────────────────────────────────────────────────────

            migrationBuilder.CreateIndex(
                name: "IX_cobros_RegistradoPorId",
                table: "cobros",
                column: "RegistradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_cobro_lineas_CobroId",
                table: "cobro_lineas",
                column: "CobroId");

            migrationBuilder.CreateIndex(
                name: "IX_comprobantes_AnuladoPorId",
                table: "comprobantes",
                column: "AnuladoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_comprobantes_EmitidoPorId",
                table: "comprobantes",
                column: "EmitidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_comprobantes_VentaId",
                table: "comprobantes",
                column: "VentaId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_cobros_users_RegistradoPorId",
                table: "cobros",
                column: "RegistradoPorId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cobros_users_RegistradoPorId",
                table: "cobros");

            migrationBuilder.DropTable(
                name: "cobro_lineas");

            migrationBuilder.DropTable(
                name: "comprobantes");

            migrationBuilder.DropIndex(
                name: "IX_cobros_RegistradoPorId",
                table: "cobros");

            migrationBuilder.DropColumn(
                name: "FechaComprobante",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "RegistradoPorId",
                table: "cobros");

            migrationBuilder.RenameColumn(
                name: "FechaConfirmacion",
                table: "ventas",
                newName: "FechaVencimiento");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "cobros",
                newName: "MetodoPago");

            migrationBuilder.RenameColumn(
                name: "MontoTotal",
                table: "cobros",
                newName: "Monto");

            migrationBuilder.RenameColumn(
                name: "Fecha",
                table: "cobros",
                newName: "FechaCobro");

            migrationBuilder.AddColumn<string>(
                name: "Referencia",
                table: "cobros",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
