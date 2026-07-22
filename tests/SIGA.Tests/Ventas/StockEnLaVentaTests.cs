using SIGA.Infrastructure.Services;
using SIGA.Tests.Infrastructure;

namespace SIGA.Tests.Ventas;

/// <summary>
/// Emitir el documento fiscal de una venta descuenta stock. Estos tests cubren la
/// vulnerabilidad V5 de CASOS_DE_PRUEBA.md (bloque B): nada impedía vender más unidades
/// de las que hay, y el stock quedaba en negativo sin que ningún paso avisara.
/// </summary>
[Collection(PostgresCollection.Name)]
public class StockEnLaVentaTests(PostgresFixture fixture)
{
    /// <summary>B2 — "La venta sin mercadería": 2 unidades en stock, se venden 10.</summary>
    [Fact]
    public async Task No_se_puede_emitir_comprobante_por_mas_unidades_de_las_que_hay_en_stock()
    {
        await using var db = await fixture.CreateDbAsync();
        var escenario = new EscenarioBuilder(db);

        var sucursal = await escenario.SucursalPorDefectoAsync();
        var usuario = await escenario.CrearUsuarioAsync(sucursalId: sucursal.Id);
        var producto = await escenario.CrearProductoConStockAsync(sucursal.Id, stockInicial: 2);
        var venta = await escenario.CrearVentaListaParaCobrarAsync(sucursal.Id, usuario.Id, producto, cantidad: 10);
        await escenario.AbrirCajaAsync(sucursal.Id, usuario.Id);

        var servicio = new VentaService(db, FakeCurrentUserContext.Global(usuario.Id), new RecordingAuditService());
        var resultado = await servicio.EmitirComprobanteAsync(venta.Id, usuario.Id);

        Assert.False(resultado.IsSuccess);
        Assert.Contains("stock", resultado.Error!, StringComparison.OrdinalIgnoreCase);

        // Y lo que de verdad importa: el stock no quedó negativo.
        Assert.Equal(2, await escenario.StockDeAsync(producto.Id, sucursal.Id));
    }

    [Fact]
    public async Task Se_puede_emitir_comprobante_cuando_hay_stock_suficiente()
    {
        await using var db = await fixture.CreateDbAsync();
        var escenario = new EscenarioBuilder(db);

        var sucursal = await escenario.SucursalPorDefectoAsync();
        var usuario = await escenario.CrearUsuarioAsync(sucursalId: sucursal.Id);
        var producto = await escenario.CrearProductoConStockAsync(sucursal.Id, stockInicial: 5);
        var venta = await escenario.CrearVentaListaParaCobrarAsync(sucursal.Id, usuario.Id, producto, cantidad: 3);
        await escenario.AbrirCajaAsync(sucursal.Id, usuario.Id);

        var servicio = new VentaService(db, FakeCurrentUserContext.Global(usuario.Id), new RecordingAuditService());
        var resultado = await servicio.EmitirComprobanteAsync(venta.Id, usuario.Id);

        Assert.True(resultado.IsSuccess, resultado.Error);
        Assert.Equal(2, await escenario.StockDeAsync(producto.Id, sucursal.Id));
    }

    /// <summary>
    /// El límite exacto: vender justo lo que hay tiene que funcionar y dejar el stock en
    /// cero. Es el caso que suele romperse cuando se corrige un bug de este tipo con un
    /// comparador equivocado (&lt; en vez de &lt;=).
    /// </summary>
    [Fact]
    public async Task Vender_exactamente_el_stock_disponible_es_valido_y_lo_deja_en_cero()
    {
        await using var db = await fixture.CreateDbAsync();
        var escenario = new EscenarioBuilder(db);

        var sucursal = await escenario.SucursalPorDefectoAsync();
        var usuario = await escenario.CrearUsuarioAsync(sucursalId: sucursal.Id);
        var producto = await escenario.CrearProductoConStockAsync(sucursal.Id, stockInicial: 4);
        var venta = await escenario.CrearVentaListaParaCobrarAsync(sucursal.Id, usuario.Id, producto, cantidad: 4);
        await escenario.AbrirCajaAsync(sucursal.Id, usuario.Id);

        var servicio = new VentaService(db, FakeCurrentUserContext.Global(usuario.Id), new RecordingAuditService());
        var resultado = await servicio.EmitirComprobanteAsync(venta.Id, usuario.Id);

        Assert.True(resultado.IsSuccess, resultado.Error);
        Assert.Equal(0, await escenario.StockDeAsync(producto.Id, sucursal.Id));
    }

    /// <summary>
    /// El stock se cuenta por sucursal: tener unidades en otro local no habilita a vender
    /// acá. Sin esta comprobación, un arreglo de V5 que mire el stock global dejaría pasar
    /// exactamente el caso que el aislamiento multi-sucursal quiere impedir.
    /// </summary>
    [Fact]
    public async Task El_stock_de_otra_sucursal_no_habilita_la_venta()
    {
        await using var db = await fixture.CreateDbAsync();
        var escenario = new EscenarioBuilder(db);

        var casaCentral = await escenario.SucursalPorDefectoAsync();
        var otraSucursal = await escenario.CrearSucursalAsync("SUC2");
        var usuario = await escenario.CrearUsuarioAsync(sucursalId: casaCentral.Id);

        // Sin stock en Casa Central; 50 unidades en la otra sucursal.
        var producto = await escenario.CrearProductoConStockAsync(casaCentral.Id, stockInicial: 0);
        await escenario.DarStockAsync(producto.Id, otraSucursal.Id, 50);

        var venta = await escenario.CrearVentaListaParaCobrarAsync(casaCentral.Id, usuario.Id, producto, cantidad: 1);
        await escenario.AbrirCajaAsync(casaCentral.Id, usuario.Id);

        var servicio = new VentaService(db, FakeCurrentUserContext.Global(usuario.Id), new RecordingAuditService());
        var resultado = await servicio.EmitirComprobanteAsync(venta.Id, usuario.Id);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(0, await escenario.StockDeAsync(producto.Id, casaCentral.Id));
        Assert.Equal(50, await escenario.StockDeAsync(producto.Id, otraSucursal.Id));
    }
}
