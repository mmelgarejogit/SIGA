using Microsoft.EntityFrameworkCore;

namespace SIGA.Tests.Infrastructure;

/// <summary>
/// Documenta y fija la línea base de toda base de test, que no arranca vacía. Viene de
/// dos fuentes distintas, y conviene no confundirlas: las <b>migraciones</b> hacen
/// backfill de datos (la 047 crea la sucursal "Casa Central"), mientras que el catálogo
/// de permisos y los roles los siembra <b>DbSeeder</b>, que en producción corre desde
/// Program.cs al arrancar la API — no las migraciones. El fixture ejecuta los dos, para
/// que la línea base sea la de un sistema recién desplegado.
///
/// Los tests de negocio se apoyan en esto, así que si cambia conviene enterarse acá y no
/// por un fallo desconcertante tres capas más arriba.
/// </summary>
[Collection(PostgresCollection.Name)]
public class BaselineTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Existe_la_sucursal_por_defecto_Casa_Central()
    {
        await using var db = await fixture.CreateDbAsync();

        var sucursal = await db.Sucursales.SingleAsync();

        Assert.Equal("CC", sucursal.Codigo);
        Assert.Equal("Casa Central", sucursal.Nombre);
        Assert.True(sucursal.IsActive);
    }

    [Fact]
    public async Task El_catalogo_de_permisos_y_los_roles_de_sistema_estan_sembrados()
    {
        await using var db = await fixture.CreateDbAsync();

        Assert.NotEmpty(await db.Permissions.ToListAsync());

        // Los tres roles de sistema se identifican por Type, no por nombre (ver ADR 0003).
        var tipos = await db.Roles
            .Where(r => r.Type != null)
            .Select(r => r.Type!)
            .ToListAsync();

        Assert.Contains("admin", tipos);
        Assert.Contains("professional", tipos);
        Assert.Contains("patient", tipos);
    }

    [Fact]
    public async Task El_rol_admin_concentra_los_permisos()
    {
        await using var db = await fixture.CreateDbAsync();

        var permisosDelAdmin = await db.RolePermissions
            .CountAsync(rp => rp.Role.Type == "admin");
        var permisosTotales = await db.Permissions.CountAsync();

        // El admin tiene todos menos ver_mis_turnos, que es del portal del paciente.
        Assert.Equal(permisosTotales - 1, permisosDelAdmin);
    }
}
