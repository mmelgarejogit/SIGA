using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _021_RefactorProveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Establecimiento",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Timbrado",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "VigenciaTimbrado",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Contacto",
                table: "Proveedores");

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                table: "Proveedores",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instagram",
                table: "Proveedores",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Facebook",
                table: "Proveedores",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ciudad",
                table: "Proveedores",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Proveedores",
                type: "character varying(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazonSocial",
                table: "Proveedores",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SitioWeb",
                table: "Proveedores",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "proveedor_contactos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProveedorId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cargo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedor_contactos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proveedor_contactos_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_proveedor_contactos_ProveedorId",
                table: "proveedor_contactos",
                column: "ProveedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "proveedor_contactos");

            migrationBuilder.DropColumn(
                name: "Ciudad",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "RazonSocial",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "SitioWeb",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "WhatsApp",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Instagram",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Facebook",
                table: "Proveedores");

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Proveedores",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Proveedores",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contacto",
                table: "Proveedores",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Establecimiento",
                table: "Proveedores",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timbrado",
                table: "Proveedores",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "VigenciaTimbrado",
                table: "Proveedores",
                type: "date",
                nullable: true);
        }
    }
}
