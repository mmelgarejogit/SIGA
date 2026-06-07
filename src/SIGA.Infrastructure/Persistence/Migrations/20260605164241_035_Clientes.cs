using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _035_Clientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonId = table.Column<int>(type: "integer", nullable: false),
                    TipoFacturacion = table.Column<int>(type: "integer", nullable: false),
                    RazonSocial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RucCiFiscal = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clientes_persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clientes_PersonId",
                table: "clientes",
                column: "PersonId",
                unique: true);

            // Migrar los datos de facturación existentes a clientes, vinculándolos
            // al Person del paciente dueño. TipoFacturacion = 0 (Física) por defecto.
            migrationBuilder.Sql("""
                INSERT INTO clientes ("PersonId", "TipoFacturacion", "RazonSocial", "RucCiFiscal", "Direccion", "Email", "Telefono", "IsActive", "CreatedAt", "UpdatedAt")
                SELECT p."PersonId", 0, df."RazonSocial", df."RucCiFiscal", df."Direccion", df."Email", df."Telefono", true, df."CreatedAt", df."UpdatedAt"
                FROM datos_facturacion df
                JOIN patients p ON p."Id" = df."PatientId";
                """);

            migrationBuilder.DropTable(
                name: "datos_facturacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "datos_facturacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    RazonSocial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RucCiFiscal = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_datos_facturacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_datos_facturacion_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_datos_facturacion_PatientId",
                table: "datos_facturacion",
                column: "PatientId",
                unique: true);

            // Best-effort: devolver a datos_facturacion los clientes cuya persona
            // corresponde a un paciente (los clientes sin paciente no se pueden mapear).
            migrationBuilder.Sql("""
                INSERT INTO datos_facturacion ("PatientId", "RazonSocial", "RucCiFiscal", "Direccion", "Email", "Telefono", "CreatedAt", "UpdatedAt")
                SELECT p."Id", c."RazonSocial", c."RucCiFiscal", c."Direccion", c."Email", c."Telefono", c."CreatedAt", c."UpdatedAt"
                FROM clientes c
                JOIN patients p ON p."PersonId" = c."PersonId";
                """);

            migrationBuilder.DropTable(
                name: "clientes");
        }
    }
}
