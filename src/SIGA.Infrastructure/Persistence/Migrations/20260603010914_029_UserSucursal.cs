using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _029_UserSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SucursalId",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_SucursalId",
                table: "users",
                column: "SucursalId");

            migrationBuilder.AddForeignKey(
                name: "FK_users_sucursales_SucursalId",
                table: "users",
                column: "SucursalId",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_sucursales_SucursalId",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_SucursalId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "users");
        }
    }
}
