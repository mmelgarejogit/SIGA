using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _019_MarcasYModelos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Productos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Productos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MarcaId",
                table: "Productos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModeloId",
                table: "Productos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Talle",
                table: "Productos",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "marcas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marcas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "modelos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MarcaId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modelos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_modelos_marcas_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "marcas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_MarcaId",
                table: "Productos",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_ModeloId",
                table: "Productos",
                column: "ModeloId");

            migrationBuilder.CreateIndex(
                name: "IX_marcas_Nombre",
                table: "marcas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_modelos_MarcaId_Nombre",
                table: "modelos",
                columns: new[] { "MarcaId", "Nombre" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_marcas_MarcaId",
                table: "Productos",
                column: "MarcaId",
                principalTable: "marcas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_modelos_ModeloId",
                table: "Productos",
                column: "ModeloId",
                principalTable: "modelos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_marcas_MarcaId",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_modelos_ModeloId",
                table: "Productos");

            migrationBuilder.DropTable(
                name: "modelos");

            migrationBuilder.DropTable(
                name: "marcas");

            migrationBuilder.DropIndex(
                name: "IX_Productos_MarcaId",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_ModeloId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "MarcaId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "ModeloId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Talle",
                table: "Productos");
        }
    }
}
