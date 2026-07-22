using Microsoft.EntityFrameworkCore;
using SIGA.Application.DTOs.Ventas;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Services;
using SIGA.Tests.Infrastructure;

namespace SIGA.Tests.Ventas;

/// <summary>
/// Confirmar una devolución saca dinero de la caja. Estos tests cubren la vulnerabilidad
/// V4 (caso C del documento de pruebas): el egreso se registraba con
/// <c>SesionCajaId = null</c> cuando no había ninguna caja abierta, con lo cual la plata
/// salía del cajón pero el movimiento no pertenecía a ninguna sesión y no aparecía en
/// ningún arqueo. El descuadre se descubría contando billetes.
///
/// El criterio correcto ya existía en el mismo servicio: la emisión del documento fiscal
/// rechaza la operación si no hay caja abierta. La devolución no seguía ese criterio.
/// </summary>
[Collection(PostgresCollection.Name)]
public class DevolucionYCajaTests(PostgresFixture fixture)
{
    private const decimal PrecioArmazon = 450_000;

    private static GestionarDevolucionRequest Confirmar => new() { Accion = "Confirmar" };

    /// <summary>Arma una venta cobrada, con comprobante emitido y una devolución pendiente.</summary>
    private static async Task<(Devolucion Devolucion, Producto Producto, Sucursal Sucursal, User Usuario)>
        PrepararDevolucionPendienteAsync(EscenarioBuilder escenario)
    {
        var sucursal = await escenario.SucursalPorDefectoAsync();
        var usuario = await escenario.CrearUsuarioAsync(sucursalId: sucursal.Id);
        var producto = await escenario.CrearProductoConStockAsync(sucursal.Id, stockInicial: 5, precioVenta: PrecioArmazon);

        var venta = await escenario.CrearVentaListaParaCobrarAsync(sucursal.Id, usuario.Id, producto, cantidad: 1);
        await escenario.RegistrarCobroAsync(venta, usuario.Id, PrecioArmazon);
        await escenario.EmitirComprobanteDirectoAsync(venta, usuario.Id);

        var devolucion = await escenario.CrearDevolucionPendienteAsync(venta, producto.Id, cantidad: 1, usuario.Id);
        return (devolucion, producto, sucursal, usuario);
    }

    [Fact]
    public async Task No_se_puede_confirmar_una_devolucion_con_reintegro_si_no_hay_caja_abierta()
    {
        await using var db = await fixture.CreateDbAsync();
        var escenario = new EscenarioBuilder(db);
        var (devolucion, _, sucursal, usuario) = await PrepararDevolucionPendienteAsync(escenario);

        // A propósito no se abre ninguna caja.
        var servicio = new VentaService(db, FakeCurrentUserContext.Global(usuario.Id), new RecordingAuditService());
        var resultado = await servicio.GestionarDevolucionAsync(devolucion.Id, Confirmar, usuario.Id, "Tester");

        Assert.False(resultado.IsSuccess);
        Assert.Contains("caja", resultado.Error!, StringComparison.OrdinalIgnoreCase);

        // Ningún movimiento de caja huérfano, y la devolución sigue pendiente.
        Assert.Empty(await db.MovimientosCaja.Where(m => m.SucursalId == sucursal.Id).ToListAsync());
        var recargada = await db.Devoluciones.AsNoTracking().FirstAsync(d => d.Id == devolucion.Id);
        Assert.Equal(EstadoDevolucion.Pendiente, recargada.Estado);
    }

    [Fact]
    public async Task Con_la_caja_abierta_el_reintegro_queda_asociado_a_la_sesion()
    {
        await using var db = await fixture.CreateDbAsync();
        var escenario = new EscenarioBuilder(db);
        var (devolucion, _, sucursal, usuario) = await PrepararDevolucionPendienteAsync(escenario);

        var sesion = await escenario.AbrirCajaAsync(sucursal.Id, usuario.Id);

        var servicio = new VentaService(db, FakeCurrentUserContext.Global(usuario.Id), new RecordingAuditService());
        var resultado = await servicio.GestionarDevolucionAsync(devolucion.Id, Confirmar, usuario.Id, "Tester");

        Assert.True(resultado.IsSuccess, resultado.Error);

        var movimiento = await db.MovimientosCaja.SingleAsync(m => m.SucursalId == sucursal.Id);
        Assert.Equal(TipoMovimientoCaja.Egreso, movimiento.Tipo);
        Assert.Equal(PrecioArmazon, movimiento.Monto);

        // El punto del test: el egreso pertenece a una sesión y por lo tanto entra al arqueo.
        Assert.Equal(sesion.Id, movimiento.SesionCajaId);
    }

    /// <summary>
    /// Una devolución que no mueve dinero (nada cobrado todavía) no necesita caja abierta:
    /// el arreglo no debe bloquear operaciones que nunca tocaron el cajón.
    /// </summary>
    [Fact]
    public async Task Una_devolucion_sin_reintegro_se_confirma_aunque_la_caja_este_cerrada()
    {
        await using var db = await fixture.CreateDbAsync();
        var escenario = new EscenarioBuilder(db);

        var sucursal = await escenario.SucursalPorDefectoAsync();
        var usuario = await escenario.CrearUsuarioAsync(sucursalId: sucursal.Id);
        var producto = await escenario.CrearProductoConStockAsync(sucursal.Id, stockInicial: 5, precioVenta: PrecioArmazon);

        // Venta a crédito sin ningún cobro registrado: TotalCobrado = 0, no hay qué reintegrar.
        var venta = await escenario.CrearVentaListaParaCobrarAsync(sucursal.Id, usuario.Id, producto, cantidad: 1);
        await escenario.EmitirComprobanteDirectoAsync(venta, usuario.Id);
        var devolucion = await escenario.CrearDevolucionPendienteAsync(venta, producto.Id, cantidad: 1, usuario.Id);

        var servicio = new VentaService(db, FakeCurrentUserContext.Global(usuario.Id), new RecordingAuditService());
        var resultado = await servicio.GestionarDevolucionAsync(devolucion.Id, Confirmar, usuario.Id, "Tester");

        Assert.True(resultado.IsSuccess, resultado.Error);
        Assert.Empty(await db.MovimientosCaja.ToListAsync());

        // El stock sí vuelve: 5 iniciales - 1 vendida (no descontada acá) + 1 devuelta.
        Assert.Equal(6, await escenario.StockDeAsync(producto.Id, sucursal.Id));
    }

    [Fact]
    public async Task Rechazar_una_devolucion_no_requiere_caja_abierta()
    {
        await using var db = await fixture.CreateDbAsync();
        var escenario = new EscenarioBuilder(db);
        var (devolucion, _, _, usuario) = await PrepararDevolucionPendienteAsync(escenario);

        var servicio = new VentaService(db, FakeCurrentUserContext.Global(usuario.Id), new RecordingAuditService());
        var resultado = await servicio.GestionarDevolucionAsync(
            devolucion.Id, new GestionarDevolucionRequest { Accion = "Rechazar" }, usuario.Id, "Tester");

        Assert.True(resultado.IsSuccess, resultado.Error);
        Assert.Empty(await db.MovimientosCaja.ToListAsync());
    }

    [Fact]
    public async Task Confirmar_una_devolucion_deja_rastro_de_auditoria()
    {
        await using var db = await fixture.CreateDbAsync();
        var escenario = new EscenarioBuilder(db);
        var (devolucion, _, sucursal, usuario) = await PrepararDevolucionPendienteAsync(escenario);
        await escenario.AbrirCajaAsync(sucursal.Id, usuario.Id);

        var auditoria = new RecordingAuditService();
        var servicio = new VentaService(db, FakeCurrentUserContext.Global(usuario.Id), auditoria);
        await servicio.GestionarDevolucionAsync(devolucion.Id, Confirmar, usuario.Id, "Tester");

        Assert.True(auditoria.Registro(AuditAccion.DevolucionAprobada));
    }
}
