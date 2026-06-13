using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SIGA.Infrastructure.Persistence;

/// <summary>
/// Factory de design-time para las herramientas de EF (migrations add / script).
/// Sólo se usa en tiempo de diseño, nunca en runtime. Permite generar migraciones
/// apuntando al proyecto Infrastructure sin compilar SIGA.Api (útil cuando el backend
/// está en ejecución y bloquea su salida). La cadena de conexión es un placeholder:
/// "migrations add" no abre conexión a la base.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var conn = Environment.GetEnvironmentVariable("SIGA_MIGRATIONS_CONN")
            ?? "Host=localhost;Port=5432;Database=siga_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(conn)
            .Options;

        return new AppDbContext(options);
    }
}
