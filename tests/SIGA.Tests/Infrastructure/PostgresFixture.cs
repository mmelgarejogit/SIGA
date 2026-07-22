using Microsoft.EntityFrameworkCore;
using SIGA.Application.Interfaces;
using SIGA.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SIGA.Tests.Infrastructure;

/// <summary>
/// Levanta un PostgreSQL real en Docker, una sola vez para toda la corrida, y aplica
/// las migraciones sobre una base "plantilla". Cada test pide su propia base con
/// <see cref="CreateDbAsync"/>, que la clona de la plantilla con
/// <c>CREATE DATABASE ... TEMPLATE ...</c> — una operación de copia de archivos en
/// Postgres, mucho más rápida que volver a correr las 90 migraciones por test.
///
/// Se usa una base real (no el proveedor InMemory de EF) porque el código depende de
/// cosas que InMemory no tiene ni valida: <c>EF.Functions.ILike</c>, la vista
/// <c>vw_stock_actual</c>, los índices únicos y las restricciones de clave foránea.
/// Con InMemory, justamente los bugs que esta suite busca pasarían en verde.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private const string TemplateDb = "siga_template";

    // Misma versión que usa el proyecto en docker-compose.yml, para no probar
    // contra un motor distinto al que corre en desarrollo y producción.
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("siga_bootstrap")
        .WithUsername("siga")
        .WithPassword("siga")
        .Build();

    private int _dbCounter;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // La plantilla se crea una sola vez y ahí se aplican las migraciones.
        await ExecuteOnPostgresAsync($"CREATE DATABASE {TemplateDb}");

        // Sin pooling: Postgres rechaza CREATE DATABASE ... TEMPLATE mientras alguien
        // siga conectado a la plantilla, y una conexión devuelta al pool de Npgsql
        // sigue abierta a nivel de servidor aunque el DbContext ya esté liberado.
        await using (var db = new AppDbContext(OptionsFor(TemplateDb, pooling: false)))
        {
            await db.Database.MigrateAsync();

            // Mismo seeder que corre Program.cs al arrancar la API: permisos, roles y
            // catálogos de configuración. Sin esto la línea base no sería la de un
            // sistema recién desplegado, sino una a medias que no existe en ningún lado.
            await DbSeeder.SeedAsync(db);
        }

        Npgsql.NpgsqlConnection.ClearAllPools();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    /// <summary>
    /// Devuelve un contexto sobre una base nueva y aislada, clonada de la plantilla.
    /// El <paramref name="currentUser"/> permite ejercitar el filtro global por
    /// sucursal; en null (el default) el contexto ve todas las sucursales, que es
    /// como se comporta un administrador o un proceso de fondo.
    /// </summary>
    public async Task<AppDbContext> CreateDbAsync(ICurrentUserContext? currentUser = null)
    {
        var name = $"siga_test_{Interlocked.Increment(ref _dbCounter)}";
        await ExecuteOnPostgresAsync($"CREATE DATABASE {name} TEMPLATE {TemplateDb}");
        return new AppDbContext(OptionsFor(name), currentUser);
    }

    /// <summary>
    /// Segundo contexto sobre la MISMA base que <paramref name="existing"/>, con otro
    /// usuario. Sirve para verificar aislamiento por sucursal (sembrar como global y
    /// leer como usuario de una sucursal) y para reproducir concurrencia entre dos
    /// sesiones distintas.
    /// </summary>
    public AppDbContext OpenAnother(AppDbContext existing, ICurrentUserContext? currentUser = null)
        => new(OptionsFor(DbNameOf(existing)), currentUser);

    private DbContextOptions<AppDbContext> OptionsFor(string database, bool pooling = true) =>
        new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionStringFor(database, pooling))
            .Options;

    private string ConnectionStringFor(string database, bool pooling = true)
    {
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = database,
            Pooling = pooling,
        };
        return builder.ConnectionString;
    }

    private static string DbNameOf(AppDbContext db) =>
        new Npgsql.NpgsqlConnectionStringBuilder(db.Database.GetConnectionString()).Database!;

    private async Task ExecuteOnPostgresAsync(string sql)
    {
        await using var conn = new Npgsql.NpgsqlConnection(ConnectionStringFor("postgres", pooling: false));
        await conn.OpenAsync();
        await using var cmd = new Npgsql.NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }
}

/// <summary>
/// Colección de xUnit que comparte un único contenedor entre todas las clases de test.
/// Sin esto, cada clase levantaría su propio Postgres y la suite tardaría minutos.
/// </summary>
[CollectionDefinition(Name)]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}
