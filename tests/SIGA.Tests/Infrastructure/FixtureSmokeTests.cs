using Microsoft.EntityFrameworkCore;
using SIGA.Domain.Entities;

namespace SIGA.Tests.Infrastructure;

/// <summary>
/// Verifica el andamiaje en sí: que el contenedor levante, que las migraciones se
/// apliquen y que cada test reciba una base aislada. Si estos fallan, cualquier otro
/// resultado de la suite es sospechoso.
/// </summary>
[Collection(PostgresCollection.Name)]
public class FixtureSmokeTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Las_migraciones_se_aplican_completas()
    {
        await using var db = await fixture.CreateDbAsync();

        // No se afirma un número exacto de migraciones a propósito: cada migración
        // nueva rompería el test sin que nada esté mal. Lo que importa es que el
        // esquema quede al día respecto del código.
        var pendientes = await db.Database.GetPendingMigrationsAsync();
        Assert.Empty(pendientes);

        var aplicadas = await db.Database.GetAppliedMigrationsAsync();
        Assert.NotEmpty(aplicadas);
    }

    [Fact]
    public async Task La_vista_de_stock_existe_y_es_consultable()
    {
        // vw_stock_actual es una vista SQL mapeada con HasNoKey(): el proveedor
        // InMemory no la tiene, y es una de las razones para usar Postgres real.
        await using var db = await fixture.CreateDbAsync();

        var filas = await db.StockActual.ToListAsync();

        Assert.Empty(filas);
    }

    [Fact]
    public async Task Cada_test_recibe_una_base_aislada()
    {
        await using var unaDb = await fixture.CreateDbAsync();
        await using var otraDb = await fixture.CreateDbAsync();

        // Ambas arrancan de la misma plantilla, que ya trae "Casa Central" (ver
        // BaselineTests). Lo que se verifica es que escribir en una no se vea en la otra.
        var antesEnLaOtra = await otraDb.Sucursales.CountAsync();

        unaDb.Sucursales.Add(new Sucursal
        {
            Nombre = "Sucursal Centro",
            Codigo = "SC",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        });
        await unaDb.SaveChangesAsync();

        Assert.Equal(antesEnLaOtra + 1, await unaDb.Sucursales.CountAsync());
        Assert.Equal(antesEnLaOtra, await otraDb.Sucursales.CountAsync());
    }

    [Fact]
    public async Task Los_indices_unicos_se_aplican_de_verdad()
    {
        // El proveedor InMemory acepta duplicados sin chistar; Postgres no.
        // Este test es el que justifica el costo de levantar Docker.
        await using var db = await fixture.CreateDbAsync();

        db.Sucursales.Add(new Sucursal { Nombre = "Una", Codigo = "DUP", IsActive = true, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        db.Sucursales.Add(new Sucursal { Nombre = "Otra", Codigo = "DUP", IsActive = true, CreatedAt = DateTime.UtcNow });
        // Mismo Codigo: debe reventar contra IX_sucursales_Codigo.

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}
