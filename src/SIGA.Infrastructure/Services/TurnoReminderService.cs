using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Options;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public sealed class TurnoReminderService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan WindowBefore = TimeSpan.FromHours(23.75);
    private static readonly TimeSpan WindowAfter  = TimeSpan.FromHours(24.25);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TurnoReminderService> _logger;

    public TurnoReminderService(IServiceScopeFactory scopeFactory, ILogger<TurnoReminderService> logger)
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
                await SendRemindersAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en TurnoReminderService");
            }

            await Task.Delay(Interval, stoppingToken)
                .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    private async Task SendRemindersAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db    = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var opts  = scope.ServiceProvider.GetRequiredService<IOptions<AppOptions>>().Value;

        var now      = DateTime.UtcNow;
        var windowLo = now.Add(WindowBefore);
        var windowHi = now.Add(WindowAfter);

        var turnos = await db.Turnos
            .Include(t => t.Professional).ThenInclude(p => p.User).ThenInclude(u => u.Person)
            .Include(t => t.Patient).ThenInclude(p => p.Person)
            .Where(t =>
                t.FechaHora >= windowLo &&
                t.FechaHora <= windowHi &&
                t.ReminderSentAt == null &&
                (t.Estado == TurnoEstado.Pendiente || t.Estado == TurnoEstado.Confirmado))
            .ToListAsync(ct);

        foreach (var turno in turnos)
        {
            var patientEmail = turno.Patient.Person.Email;
            if (string.IsNullOrWhiteSpace(patientEmail)) continue;

            try
            {
                var (subject, body) = BuildReminderEmail(
                    $"{turno.Patient.Person.FirstName} {turno.Patient.Person.LastName}",
                    $"{turno.Professional.User.Person.FirstName} {turno.Professional.User.Person.LastName}",
                    turno.FechaHora, turno.Motivo);

                await email.SendAsync(patientEmail, subject, body);

                turno.ReminderSentAt = now;
                _logger.LogInformation("Recordatorio enviado al turno {TurnoId}", turno.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo enviar recordatorio para turno {TurnoId}", turno.Id);
            }
        }

        if (turnos.Count > 0)
            await db.SaveChangesAsync(ct);
    }

    private static (string Subject, string Body) BuildReminderEmail(
        string pacienteNombre, string profesionalNombre, DateTime fechaHora, string? motivo)
    {
        var localFecha = fechaHora;
        var fechaStr   = localFecha.ToString("dddd d 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-AR"));
        var horaStr    = localFecha.ToString("HH:mm");

        var motivoRow = !string.IsNullOrWhiteSpace(motivo)
            ? $"""<tr><td style="color:#6B7280;padding:4px 0;width:120px">Motivo</td><td style="color:#111827;font-weight:600">{motivo}</td></tr>"""
            : "";

        var body = $"""
            <!DOCTYPE html>
            <html lang="es">
            <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
            <body style="margin:0;padding:0;background-color:#F7F9FE;font-family:Arial,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0">
                <tr>
                  <td align="center" style="padding:40px 20px;">
                    <table width="560" cellpadding="0" cellspacing="0"
                      style="background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.06);">
                      <tr>
                        <td style="background:#92400E;padding:28px 40px;">
                          <p style="margin:0;color:#ffffff;font-size:22px;font-weight:900;">SIGA Óptica</p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:40px;">
                          <h2 style="margin:0 0 8px;color:#181C20;font-size:22px;">Recordatorio: tu turno es mañana</h2>
                          <p style="color:#444653;margin:0 0 28px;">
                            Te recordamos que tenés un turno agendado mañana en SIGA Óptica.
                          </p>

                          <table cellpadding="0" cellspacing="0"
                            style="width:100%;background:#F7F9FE;border-radius:12px;padding:20px;margin-bottom:28px;">
                            <tr>
                              <td colspan="2" style="padding-bottom:12px;font-size:11px;font-weight:700;
                                  letter-spacing:.08em;text-transform:uppercase;color:#9CA3AF;">
                                Datos del turno
                              </td>
                            </tr>
                            <tr>
                              <td style="color:#6B7280;padding:4px 0;width:120px">Paciente</td>
                              <td style="color:#111827;font-weight:600">{pacienteNombre}</td>
                            </tr>
                            <tr>
                              <td style="color:#6B7280;padding:4px 0">Profesional</td>
                              <td style="color:#111827;font-weight:600">{profesionalNombre}</td>
                            </tr>
                            <tr>
                              <td style="color:#6B7280;padding:4px 0">Fecha</td>
                              <td style="color:#111827;font-weight:600">{fechaStr}</td>
                            </tr>
                            <tr>
                              <td style="color:#6B7280;padding:4px 0">Hora</td>
                              <td style="color:#111827;font-weight:600">{horaStr} hs</td>
                            </tr>
                            {motivoRow}
                          </table>

                          <p style="color:#757684;font-size:12px;margin:0;">
                            Si no podés asistir, comunicate con nosotros lo antes posible.
                          </p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;

        return ("Recordatorio de turno — SIGA Óptica", body);
    }
}
