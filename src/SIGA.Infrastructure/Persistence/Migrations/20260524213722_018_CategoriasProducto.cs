using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _018_CategoriasProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoriaProductoId",
                table: "Productos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "categorias_producto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias_producto", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaProductoId",
                table: "Productos",
                column: "CategoriaProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_categorias_producto_Nombre",
                table: "categorias_producto",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_categorias_producto_CategoriaProductoId",
                table: "Productos",
                column: "CategoriaProductoId",
                principalTable: "categorias_producto",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_categorias_producto_CategoriaProductoId",
                table: "Productos");

            migrationBuilder.DropTable(
                name: "categorias_producto");

            migrationBuilder.DropIndex(
                name: "IX_Productos_CategoriaProductoId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "CategoriaProductoId",
                table: "Productos");
        }
    }
}
