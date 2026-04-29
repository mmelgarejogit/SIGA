using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _008_ClinicaModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "consultas_clinicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    ProfessionalId = table.Column<int>(type: "integer", nullable: false),
                    CitaId = table.Column<int>(type: "integer", nullable: true),
                    FechaConsulta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Anamnesis = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ExamenFisico = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DiagnosticoPrincipal = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DiagnosticoSecundario = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PlanTratamiento = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consultas_clinicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_consultas_clinicas_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_consultas_clinicas_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "recetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConsultaClinicaId = table.Column<int>(type: "integer", nullable: false),
                    FechaEmision = table.Column<DateOnly>(type: "date", nullable: false),
                    OdEsferico = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    OdCilindro = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    OdEje = table.Column<int>(type: "integer", nullable: true),
                    OdAdicion = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    OiEsferico = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    OiCilindro = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    OiEje = table.Column<int>(type: "integer", nullable: true),
                    OiAdicion = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    DistanciaInterpupilar = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    AvSinCorreccion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AvConCorreccion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recetas_consultas_clinicas_ConsultaClinicaId",
                        column: x => x.ConsultaClinicaId,
                        principalTable: "consultas_clinicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_consultas_clinicas_PatientId",
                table: "consultas_clinicas",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_consultas_clinicas_ProfessionalId",
                table: "consultas_clinicas",
                column: "ProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_recetas_ConsultaClinicaId",
                table: "recetas",
                column: "ConsultaClinicaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recetas");

            migrationBuilder.DropTable(
                name: "consultas_clinicas");
        }
    }
}
