using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _016_EstadoConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Crear tabla estados_config
            migrationBuilder.CreateTable(
                name: "estados_config",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Entidad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CodigoInterno = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EsProtegido = table.Column<bool>(type: "boolean", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados_config", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_estados_config_Entidad_Nombre",
                table: "estados_config",
                columns: new[] { "Entidad", "Nombre" },
                unique: true);

            // 2. Sembrar estados iniciales
            migrationBuilder.Sql(@"
                INSERT INTO estados_config (""Entidad"", ""Nombre"", ""Color"", ""CodigoInterno"", ""EsProtegido"", ""Orden"") VALUES
                ('Turno',    'Pendiente',  '#F59E0B', 'Pendiente',  true,  1),
                ('Turno',    'Completado', '#10B981', 'Completado', true,  2),
                ('Turno',    'Cancelado',  '#EF4444', 'Cancelado',  true,  3),
                ('Pedido',   'Pendiente',  '#F59E0B', 'Pendiente',  true,  1),
                ('Pedido',   'Enviado',    '#3B82F6', 'Enviado',    true,  2),
                ('Pedido',   'Recibido',   '#10B981', 'Recibido',   true,  3),
                ('Pedido',   'Cancelado',  '#EF4444', 'Cancelado',  true,  4),
                ('Consulta', 'Pendiente',  '#F59E0B', 'Pendiente',  false, 1),
                ('Consulta', 'Abierta',    '#3B82F6', 'Abierta',    false, 2),
                ('Consulta', 'Cerrada',    '#10B981', 'Cerrada',    false, 3),
                ('Consulta', 'Cancelada',  '#EF4444', 'Cancelada',  false, 4);
            ");

            // 3. Agregar EstadoConfigId a PedidosProveedor (antes de dropear Estado)
            migrationBuilder.AddColumn<int>(
                name: "EstadoConfigId",
                table: "PedidosProveedor",
                type: "integer",
                nullable: true);

            // 4. Migrar datos: mapear string Estado -> FK
            migrationBuilder.Sql(@"
                UPDATE ""PedidosProveedor"" p
                SET ""EstadoConfigId"" = e.""Id""
                FROM estados_config e
                WHERE e.""Entidad"" = 'Pedido'
                  AND e.""CodigoInterno"" = p.""Estado"";
            ");

            // 5. Eliminar columna string Estado
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "PedidosProveedor");

            // 6. Agregar EstadoConfigId a consultas_clinicas y EstadoCustomId a turnos
            migrationBuilder.AddColumn<int>(
                name: "EstadoConfigId",
                table: "consultas_clinicas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstadoCustomId",
                table: "turnos",
                type: "integer",
                nullable: true);

            // 7. Índices
            migrationBuilder.CreateIndex(
                name: "IX_PedidosProveedor_EstadoConfigId",
                table: "PedidosProveedor",
                column: "EstadoConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_consultas_clinicas_EstadoConfigId",
                table: "consultas_clinicas",
                column: "EstadoConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_turnos_EstadoCustomId",
                table: "turnos",
                column: "EstadoCustomId");

            // 8. Foreign keys
            migrationBuilder.AddForeignKey(
                name: "FK_consultas_clinicas_estados_config_EstadoConfigId",
                table: "consultas_clinicas",
                column: "EstadoConfigId",
                principalTable: "estados_config",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PedidosProveedor_estados_config_EstadoConfigId",
                table: "PedidosProveedor",
                column: "EstadoConfigId",
                principalTable: "estados_config",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_turnos_estados_config_EstadoCustomId",
                table: "turnos",
                column: "EstadoCustomId",
                principalTable: "estados_config",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_consultas_clinicas_estados_config_EstadoConfigId",
                table: "consultas_clinicas");

            migrationBuilder.DropForeignKey(
                name: "FK_PedidosProveedor_estados_config_EstadoConfigId",
                table: "PedidosProveedor");

            migrationBuilder.DropForeignKey(
                name: "FK_turnos_estados_config_EstadoCustomId",
                table: "turnos");

            migrationBuilder.DropIndex(
                name: "IX_turnos_EstadoCustomId",
                table: "turnos");

            migrationBuilder.DropIndex(
                name: "IX_PedidosProveedor_EstadoConfigId",
                table: "PedidosProveedor");

            migrationBuilder.DropIndex(
                name: "IX_consultas_clinicas_EstadoConfigId",
                table: "consultas_clinicas");

            migrationBuilder.DropColumn(
                name: "EstadoCustomId",
                table: "turnos");

            migrationBuilder.DropColumn(
                name: "EstadoConfigId",
                table: "PedidosProveedor");

            migrationBuilder.DropColumn(
                name: "EstadoConfigId",
                table: "consultas_clinicas");

            migrationBuilder.DropTable(
                name: "estados_config");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "PedidosProveedor",
                type: "text",
                nullable: false,
                defaultValue: "Pendiente");
        }
    }
}
