using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIGA.Application.Interfaces;
using SIGA.Domain.Security;
using SIGA.Infrastructure.Options;
using SIGA.Infrastructure.Persistence;
using SIGA.Infrastructure.Security;
using SIGA.Infrastructure.Services;

namespace SIGA.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(
                config.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null)));

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IProfessionalService, ProfessionalService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IConsultaClinicaService, ConsultaClinicaService>();
        services.AddScoped<IRecetaService, RecetaService>();
        services.AddScoped<IRecetaPdfGenerator, RecetaPdfGenerator>();
        services.AddScoped<IConfiguracionNegocioService, ConfiguracionNegocioService>();
        services.AddScoped<IEmailService, ResendEmailService>();
        services.AddScoped<IHCaptchaService, HCaptchaService>();
        services.AddScoped<ISucursalService, SucursalService>();
        services.AddScoped<ITransferenciaStockService, TransferenciaStockService>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        services.Configure<ResendOptions>(config.GetSection("Resend"));
        services.Configure<HCaptchaOptions>(config.GetSection("HCaptcha"));
        services.Configure<AppOptions>(config.GetSection("App"));

        services.AddHttpClient("resend", (_, client) =>
        {
            var apiKey = config["Resend:ApiKey"] ?? string.Empty;
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        });

        services.AddHttpClient("hcaptcha");
        services.AddScoped<IEspecialidadService, EspecialidadService>();
        services.AddScoped<IEstadoConfigService, EstadoConfigService>();
        services.AddScoped<IHorarioProfesionalService, HorarioProfesionalService>();
        services.AddScoped<ITurnoService, TurnoService>();
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IMovimientoStockPdfGenerator, MovimientoStockPdfGenerator>();
        services.AddScoped<IMotivoMovimientoService, MotivoMovimientoService>();
        services.AddScoped<IMarcaService, MarcaService>();
        services.AddScoped<ITipoLenteService, TipoLenteService>();
        services.AddScoped<ITratamientoService, TratamientoService>();
        services.AddScoped<IServicioService, ServicioService>();
        services.AddScoped<IProveedorService, ProveedorService>();
        services.AddScoped<IComprasService, ComprasService>();
        services.AddScoped<IFacturasCompraService, FacturasCompraService>();
        services.AddScoped<IRecepcionesService, RecepcionesService>();
        services.AddScoped<IEgresoService, EgresoService>();
        services.AddScoped<IEmpleadoService, EmpleadoService>();
        services.AddScoped<IVentaService, VentaService>();
        services.AddScoped<ILaboratorioService, LaboratorioService>();
        services.AddScoped<ICajaService, CajaService>();
        services.AddScoped<ITimbradoService, TimbradoService>();
        services.AddScoped<IStockLoteService, StockLoteService>();
        services.AddScoped<IUbicacionService, UbicacionService>();
        services.AddScoped<IReporteService, ReporteService>();
        services.AddScoped<IReporteOperativoService, ReporteOperativoService>();
        services.AddScoped<IReporteExporter, ReporteExporter>();
        services.AddScoped<INotificacionInternaService, NotificacionInternaService>();
        services.AddScoped<INotificacionPreferenciaService, NotificacionPreferenciaService>();
        services.AddScoped<IAuditService, AuditService>();

        services.AddHostedService<TurnoReminderService>();
        services.AddHostedService<StockBajoNotificadorService>();

        return services;
    }
}
