using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTurnoNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfirmationToken",
                table: "turnos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderSentAt",
                table: "turnos",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmationToken",
                table: "turnos");

            migrationBuilder.DropColumn(
                name: "ReminderSentAt",
                table: "turnos");
        }
    }
}
