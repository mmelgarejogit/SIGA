using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _047_Sucursales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "sucursales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Direccion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CiudadId = table.Column<int>(type: "integer", nullable: true),
                    Establecimiento = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sucursales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sucursales_ciudades_CiudadId",
                        column: x => x.CiudadId,
                        principalTable: "ciudades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_SucursalId",
                table: "users",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_sucursales_CiudadId",
                table: "sucursales",
                column: "CiudadId");

            migrationBuilder.CreateIndex(
                name: "IX_sucursales_Codigo",
                table: "sucursales",
                column: "Codigo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_users_sucursales_SucursalId",
                table: "users",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ── Backfill ──────────────────────────────────────────────────────────────
            // 1) Sucursal por defecto "Casa Central" (idempotente por Código).
            migrationBuilder.Sql("""
                INSERT INTO sucursales ("Nombre", "Codigo", "IsActive", "CreatedAt", "UpdatedAt")
                SELECT 'Casa Central', 'CC', true, now(), now()
                WHERE NOT EXISTS (SELECT 1 FROM sucursales WHERE "Codigo" = 'CC');
                """);

            // 2) Asignar a Casa Central solo al STAFF existente (los que operan en un local).
            //    Quedan con SucursalId = null (usuarios globales) el admin y los PACIENTES:
            //    el paciente no pertenece a una sucursal, elige una al reservar el turno.
            migrationBuilder.Sql("""
                UPDATE users
                SET "SucursalId" = (SELECT "Id" FROM sucursales WHERE "Codigo" = 'CC')
                WHERE "Id" NOT IN (
                    SELECT ur."UserId"
                    FROM user_roles ur
                    JOIN roles r ON r."Id" = ur."RoleId"
                    WHERE r."Type" IN ('admin', 'patient')
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_sucursales_SucursalId",
                table: "users");

            migrationBuilder.DropTable(
                name: "sucursales");

            migrationBuilder.DropIndex(
                name: "IX_users_SucursalId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "users");
        }
    }
}
