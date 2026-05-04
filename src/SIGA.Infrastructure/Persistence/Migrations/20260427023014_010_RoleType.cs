using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _010_RoleType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "roles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_Type",
                table: "roles",
                column: "Type",
                unique: true,
                filter: "\"Type\" IS NOT NULL");

            migrationBuilder.Sql(@"
                -- Assign type only to the lowest-Id row matching each system role (avoids unique violation when duplicates exist)
                UPDATE roles SET ""Type"" = 'admin'
                WHERE ""Id"" = (SELECT MIN(""Id"") FROM roles WHERE ""Name"" IN ('Admin', 'Administrador') AND ""Type"" IS NULL);

                UPDATE roles SET ""Type"" = 'professional'
                WHERE ""Id"" = (SELECT MIN(""Id"") FROM roles WHERE ""Name"" IN ('Professional', 'Profesional') AND ""Type"" IS NULL);

                UPDATE roles SET ""Type"" = 'patient'
                WHERE ""Id"" = (SELECT MIN(""Id"") FROM roles WHERE ""Name"" IN ('Patient', 'Paciente') AND ""Type"" IS NULL);

                -- Re-point user_roles from duplicate rows to the canonical (lowest Id) row
                UPDATE user_roles ur
                SET ""RoleId"" = canonical.""Id""
                FROM roles dup
                JOIN (SELECT ""Type"", MIN(""Id"") AS ""Id"" FROM roles WHERE ""Type"" IS NOT NULL GROUP BY ""Type"") canonical
                    ON canonical.""Type"" = dup.""Type""
                WHERE ur.""RoleId"" = dup.""Id""
                  AND dup.""Id"" > canonical.""Id"";

                -- Re-point role_permissions from duplicate rows to the canonical row
                UPDATE role_permissions rp
                SET ""RoleId"" = canonical.""Id""
                FROM roles dup
                JOIN (SELECT ""Type"", MIN(""Id"") AS ""Id"" FROM roles WHERE ""Type"" IS NOT NULL GROUP BY ""Type"") canonical
                    ON canonical.""Type"" = dup.""Type""
                WHERE rp.""RoleId"" = dup.""Id""
                  AND dup.""Id"" > canonical.""Id"";

                -- Remove any role_permissions duplicates introduced by the merge
                DELETE FROM role_permissions a
                USING role_permissions b
                WHERE a.""RoleId"" = b.""RoleId""
                  AND a.""PermissionId"" = b.""PermissionId""
                  AND a.ctid > b.ctid;

                -- Delete non-canonical (duplicate) system role rows
                DELETE FROM roles
                WHERE ""Type"" IS NULL
                  AND ""Name"" IN ('Admin', 'Administrador', 'Professional', 'Profesional', 'Patient', 'Paciente');

                -- Rename canonical system roles to Spanish display names
                UPDATE roles SET ""Name"" = 'Administrador' WHERE ""Type"" = 'admin';
                UPDATE roles SET ""Name"" = 'Profesional'   WHERE ""Type"" = 'professional';
                UPDATE roles SET ""Name"" = 'Paciente'      WHERE ""Type"" = 'patient';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_roles_Type",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "roles");
        }
    }
}
