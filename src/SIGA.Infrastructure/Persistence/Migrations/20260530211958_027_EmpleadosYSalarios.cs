using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _027_EmpleadosYSalarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmpleadoId",
                table: "egresos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalarioEmpleado_Periodo",
                table: "egresos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cargos_empleado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cargos_empleado", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "empleados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CargoId = table.Column<int>(type: "integer", nullable: false),
                    FechaIngreso = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaEgreso = table.Column<DateOnly>(type: "date", nullable: true),
                    SalarioBase = table.Column<decimal>(type: "numeric(18,0)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empleados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_empleados_cargos_empleado_CargoId",
                        column: x => x.CargoId,
                        principalTable: "cargos_empleado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_empleados_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_egresos_EmpleadoId",
                table: "egresos",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_empleados_CargoId",
                table: "empleados",
                column: "CargoId");

            migrationBuilder.CreateIndex(
                name: "IX_empleados_UserId",
                table: "empleados",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_egresos_empleados_EmpleadoId",
                table: "egresos",
                column: "EmpleadoId",
                principalTable: "empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_egresos_empleados_EmpleadoId",
                table: "egresos");

            migrationBuilder.DropTable(
                name: "empleados");

            migrationBuilder.DropTable(
                name: "cargos_empleado");

            migrationBuilder.DropIndex(
                name: "IX_egresos_EmpleadoId",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "EmpleadoId",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "SalarioEmpleado_Periodo",
                table: "egresos");
        }
    }
}
