using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIGA.Application.Interfaces;
using SIGA.Infrastructure.Persistence;
using SIGA.Domain.Security;
using SIGA.Infrastructure.Security;
using SIGA.Infrastructure.Services;

namespace SIGA.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IProfessionalService, ProfessionalService>();
        services.AddScoped<IPatientService, PatientService>();

        return services;
    }
}
