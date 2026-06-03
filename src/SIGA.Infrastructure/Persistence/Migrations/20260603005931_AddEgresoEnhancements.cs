using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEgresoEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Periodo",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "SalarioEmpleado_Periodo",
                table: "egresos");

            migrationBuilder.AddColumn<Guid>(
                name: "SucursalId",
                table: "movimientos_caja",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "NroComprobante",
                table: "egresos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AprobadoPorUserId",
                table: "egresos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreadoPorUserId",
                table: "egresos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "egresos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PeriodoAnio",
                table: "egresos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PeriodoMes",
                table: "egresos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalarioEmpleado_PeriodoAnio",
                table: "egresos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalarioEmpleado_PeriodoMes",
                table: "egresos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SucursalId",
                table: "egresos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "egresos_pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EgresoId = table.Column<int>(type: "integer", nullable: false),
                    FechaPago = table.Column<DateOnly>(type: "date", nullable: false),
                    MetodoPago = table.Column<int>(type: "integer", nullable: false),
                    NumeroComprobante = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RegistradoPorUserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_egresos_pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_egresos_pagos_egresos_EgresoId",
                        column: x => x.EgresoId,
                        principalTable: "egresos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_egresos_pagos_users_RegistradoPorUserId",
                        column: x => x.RegistradoPorUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_caja_SucursalId",
                table: "movimientos_caja",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_egresos_AprobadoPorUserId",
                table: "egresos",
                column: "AprobadoPorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_egresos_CreadoPorUserId",
                table: "egresos",
                column: "CreadoPorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_egresos_SucursalId",
                table: "egresos",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_egresos_pagos_EgresoId",
                table: "egresos_pagos",
                column: "EgresoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_egresos_pagos_RegistradoPorUserId",
                table: "egresos_pagos",
                column: "RegistradoPorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_egresos_sucursales_SucursalId",
                table: "egresos",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_egresos_users_AprobadoPorUserId",
                table: "egresos",
                column: "AprobadoPorUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_egresos_users_CreadoPorUserId",
                table: "egresos",
                column: "CreadoPorUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_movimientos_caja_sucursales_SucursalId",
                table: "movimientos_caja",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_egresos_sucursales_SucursalId",
                table: "egresos");

            migrationBuilder.DropForeignKey(
                name: "FK_egresos_users_AprobadoPorUserId",
                table: "egresos");

            migrationBuilder.DropForeignKey(
                name: "FK_egresos_users_CreadoPorUserId",
                table: "egresos");

            migrationBuilder.DropForeignKey(
                name: "FK_movimientos_caja_sucursales_SucursalId",
                table: "movimientos_caja");

            migrationBuilder.DropTable(
                name: "egresos_pagos");

            migrationBuilder.DropIndex(
                name: "IX_movimientos_caja_SucursalId",
                table: "movimientos_caja");

            migrationBuilder.DropIndex(
                name: "IX_egresos_AprobadoPorUserId",
                table: "egresos");

            migrationBuilder.DropIndex(
                name: "IX_egresos_CreadoPorUserId",
                table: "egresos");

            migrationBuilder.DropIndex(
                name: "IX_egresos_SucursalId",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "movimientos_caja");

            migrationBuilder.DropColumn(
                name: "AprobadoPorUserId",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "CreadoPorUserId",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "PeriodoAnio",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "PeriodoMes",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "SalarioEmpleado_PeriodoAnio",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "SalarioEmpleado_PeriodoMes",
                table: "egresos");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "egresos");

            migrationBuilder.AlterColumn<string>(
                name: "NroComprobante",
                table: "egresos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Periodo",
                table: "egresos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalarioEmpleado_Periodo",
                table: "egresos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
