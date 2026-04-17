using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _003_PatientDirectPersonLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_patients_users_UserId",
                table: "patients");

            migrationBuilder.DropIndex(
                name: "IX_persons_Email",
                table: "persons");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "persons",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(320)",
                oldMaxLength: 320);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "patients",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "patients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PersonId",
                table: "patients",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_persons_Email",
                table: "persons",
                column: "Email",
                unique: true)
                .Annotation("Npgsql:NullsDistinct", true);

            migrationBuilder.CreateIndex(
                name: "IX_patients_PersonId",
                table: "patients",
                column: "PersonId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_patients_persons_PersonId",
                table: "patients",
                column: "PersonId",
                principalTable: "persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_patients_users_UserId",
                table: "patients",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_patients_persons_PersonId",
                table: "patients");

            migrationBuilder.DropForeignKey(
                name: "FK_patients_users_UserId",
                table: "patients");

            migrationBuilder.DropIndex(
                name: "IX_persons_Email",
                table: "persons");

            migrationBuilder.DropIndex(
                name: "IX_patients_PersonId",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "patients");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "persons",
                type: "character varying(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(320)",
                oldMaxLength: 320,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "patients",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_persons_Email",
                table: "persons",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_patients_users_UserId",
                table: "patients",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
