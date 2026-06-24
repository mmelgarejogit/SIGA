using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _025_RecepcionEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaVencimiento",
                table: "RecepcionesMercaderiaItems",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Lote",
                table: "RecepcionesMercaderiaItems",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "RecepcionesMercaderiaItems",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "RecepcionesMercaderia",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FacturaCompraId",
                table: "RecepcionesMercaderia",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaRecepcion",
                table: "RecepcionesMercaderia",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "RecepcionesMercaderia",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesMercaderia_FacturaCompraId",
                table: "RecepcionesMercaderia",
                column: "FacturaCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesMercaderia_UserId",
                table: "RecepcionesMercaderia",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecepcionesMercaderia_egresos_FacturaCompraId",
                table: "RecepcionesMercaderia",
                column: "FacturaCompraId",
                principalTable: "egresos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RecepcionesMercaderia_users_UserId",
                table: "RecepcionesMercaderia",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecepcionesMercaderia_egresos_FacturaCompraId",
                table: "RecepcionesMercaderia");

            migrationBuilder.DropForeignKey(
                name: "FK_RecepcionesMercaderia_users_UserId",
                table: "RecepcionesMercaderia");

            migrationBuilder.DropIndex(
                name: "IX_RecepcionesMercaderia_FacturaCompraId",
                table: "RecepcionesMercaderia");

            migrationBuilder.DropIndex(
                name: "IX_RecepcionesMercaderia_UserId",
                table: "RecepcionesMercaderia");

            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                table: "RecepcionesMercaderiaItems");

            migrationBuilder.DropColumn(
                name: "Lote",
                table: "RecepcionesMercaderiaItems");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "RecepcionesMercaderiaItems");

            migrationBuilder.DropColumn(
                name: "FacturaCompraId",
                table: "RecepcionesMercaderia");

            migrationBuilder.DropColumn(
                name: "FechaRecepcion",
                table: "RecepcionesMercaderia");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RecepcionesMercaderia");

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "RecepcionesMercaderia",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
