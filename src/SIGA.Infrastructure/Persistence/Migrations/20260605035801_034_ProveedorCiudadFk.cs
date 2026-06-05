using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _034_ProveedorCiudadFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Agregar columna FK (nullable)
            migrationBuilder.AddColumn<int>(
                name: "CiudadId",
                table: "Proveedores",
                type: "integer",
                nullable: true);

            // 2. Mapear ciudades legacy (texto) al id correspondiente por nombre
            migrationBuilder.Sql("""
                UPDATE "Proveedores" p
                SET "CiudadId" = c."Id"
                FROM ciudades c
                WHERE p."Ciudad" IS NOT NULL
                  AND lower(trim(p."Ciudad")) = lower(c."Nombre");
                """);

            // 3. Eliminar la columna de texto legacy
            migrationBuilder.DropColumn(
                name: "Ciudad",
                table: "Proveedores");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_CiudadId",
                table: "Proveedores",
                column: "CiudadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Proveedores_ciudades_CiudadId",
                table: "Proveedores",
                column: "CiudadId",
                principalTable: "ciudades",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proveedores_ciudades_CiudadId",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Proveedores_CiudadId",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "CiudadId",
                table: "Proveedores");

            migrationBuilder.AddColumn<string>(
                name: "Ciudad",
                table: "Proveedores",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
