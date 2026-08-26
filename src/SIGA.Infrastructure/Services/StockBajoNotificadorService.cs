using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

/// <summary>
/// El bajo stock se calcula al vuelo (no hay un evento único: los movimientos "Aprobado"
/// se crean desde Compras/Ventas/Recepciones/Conteos/Transferencias). En vez de instrumentar
/// cada uno de esos servicios, este poller evalúa el cruce periódicamente — mismo patrón que
/// TurnoReminderService.
/// </summary>
public sealed class StockBajoNotificadorService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StockBajoNotificadorService> _logger;

    public StockBajoNotificadorService(IServiceScopeFactory scopeFactory, ILogger<StockBajoNotificadorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EvaluarStockBajoAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en StockBajoNotificadorService");
            }

            await Task.Delay(Interval, stoppingToken)
                .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    private async Task EvaluarStockBajoAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db            = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notificacion  = scope.ServiceProvider.GetRequiredService<INotificacionInternaService>();

        var bajoStock = await (
            from s in db.StockActual
            join c in db.ProductosStockConfig on s.ProductoId equals c.ProductoId
            join p in db.Productos on s.ProductoId equals p.Id
            where p.IsActive && s.StockActual <= c.StockMinimo
            select new { s.ProductoId, s.SucursalId, s.StockActual, c.StockMinimo, p.Nombre }
        ).ToListAsync(ct);

        if (bajoStock.Count == 0) return;

        var yaNotificados = await db.NotificacionesInternas
            .Where(n => n.Tipo == TipoNotificacion.BajoStock && !n.Leido && n.EntidadOrigenTipo == "Producto")
            .Select(n => new { n.EntidadOrigenId, n.DestinatarioSucursalId })
            .ToListAsync(ct);

        var yaNotificadosSet = yaNotificados
            .Select(n => (n.EntidadOrigenId, n.DestinatarioSucursalId))
            .ToHashSet();

        foreach (var item in bajoStock)
        {
            if (yaNotificadosSet.Contains((item.ProductoId, item.SucursalId)))
                continue;

            await notificacion.CrearAsync(
                tipo: TipoNotificacion.BajoStock,
                mensaje: $"\"{item.Nombre}\" está bajo el mínimo (stock: {item.StockActual}, mínimo: {item.StockMinimo}).",
                entidadOrigenTipo: "Producto",
                entidadOrigenId: item.ProductoId,
                destinatarioSucursalId: item.SucursalId);
        }
    }
}
