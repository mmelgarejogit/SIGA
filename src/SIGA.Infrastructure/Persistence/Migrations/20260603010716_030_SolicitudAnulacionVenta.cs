using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _030_SolicitudAnulacionVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitudesAnulacionVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    Motivo = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    SolicitadoPorId = table.Column<int>(type: "integer", nullable: false),
                    SolicitadoPorNombre = table.Column<string>(type: "text", nullable: false),
                    EstadoPrevioVenta = table.Column<string>(type: "text", nullable: false),
                    RevisadoPorNombre = table.Column<string>(type: "text", nullable: true),
                    ObservacionesRevision = table.Column<string>(type: "text", nullable: true),
                    FechaRevision = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesAnulacionVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesAnulacionVenta_ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAnulacionVenta_VentaId",
                table: "SolicitudesAnulacionVenta",
                column: "VentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesAnulacionVenta");
        }
    }
}
