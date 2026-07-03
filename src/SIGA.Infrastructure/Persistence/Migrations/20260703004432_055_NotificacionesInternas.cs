using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _055_NotificacionesInternas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notificaciones_internas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DestinatarioUsuarioId = table.Column<int>(type: "integer", nullable: true),
                    DestinatarioSucursalId = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Mensaje = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EntidadOrigenTipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EntidadOrigenId = table.Column<int>(type: "integer", nullable: true),
                    Leido = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaLectura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificaciones_internas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notificaciones_internas_sucursales_DestinatarioSucursalId",
                        column: x => x.DestinatarioSucursalId,
                        principalTable: "sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notificaciones_internas_users_DestinatarioUsuarioId",
                        column: x => x.DestinatarioUsuarioId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notificaciones_internas_DestinatarioSucursalId_Leido",
                table: "notificaciones_internas",
                columns: new[] { "DestinatarioSucursalId", "Leido" });

            migrationBuilder.CreateIndex(
                name: "IX_notificaciones_internas_DestinatarioUsuarioId",
                table: "notificaciones_internas",
                column: "DestinatarioUsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notificaciones_internas");
        }
    }
}
