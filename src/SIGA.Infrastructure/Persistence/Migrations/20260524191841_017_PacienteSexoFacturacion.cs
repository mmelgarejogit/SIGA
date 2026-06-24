using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _017_PacienteSexoFacturacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sexo",
                table: "persons",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "datos_facturacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    RucCiFiscal = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RazonSocial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "datos_facturacion");

            migrationBuilder.DropColumn(
                name: "Sexo",
                table: "persons");
        }
    }
}
