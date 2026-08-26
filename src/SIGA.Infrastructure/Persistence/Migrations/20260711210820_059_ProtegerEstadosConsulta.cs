using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _059_ProtegerEstadosConsulta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Los 4 estados de Consulta se sembraron con EsProtegido=false (a diferencia de
            // Turno/Pedido, que sí protegen todos sus estados por defecto). Sin protección,
            // "Abierta" puede borrarse desde Estados de Configuración cuando no hay ninguna
            // consulta abierta en ese momento — y ConsultaClinicaService.CreateAsync busca
            // CodigoInterno == "Abierta" sin fallback, rompiendo la creación de consultas.
            migrationBuilder.Sql("""
                UPDATE "estados_config" SET "EsProtegido" = true WHERE "Entidad" = 'Consulta';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "estados_config" SET "EsProtegido" = false WHERE "Entidad" = 'Consulta';
                """);
        }
    }
}
