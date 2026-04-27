using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SIGA.Infrastructure;
using SIGA.Infrastructure.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==============================
// 1️⃣ Servicios
// ==============================
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

var permissionPolicies = new[]
{
    "ver_pacientes",     "crear_paciente",      "editar_paciente",    "desactivar_paciente",
    "ver_profesionales", "crear_profesional",   "editar_profesional",
    "ver_especialidades", "gestionar_especialidades",
    "ver_usuarios",      "editar_usuario",
    "ver_roles",         "crear_rol",           "editar_rol",         "eliminar_rol",
    "ver_calendario",
    "ver_historia_clinica",
    "ver_inventario",
    "ver_ventas",
    "ver_reportes",
};

builder.Services.AddAuthorization(options =>
{
    foreach (var perm in permissionPolicies)
        options.AddPolicy(perm, policy => policy.RequireClaim("permission", perm));
});

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SIGA API",
        Version = "v1",
        Description = "Sistema de Gestión para Óptica"
    });

    // Allow sending JWT token from Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token here"
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", doc),
            []
        }
    });
});

// ==============================
// 2️⃣ Construcción app
// ==============================
var app = builder.Build();

// ==============================
// 3️⃣ Middlewares
// ==============================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ==============================
// 4️⃣ Seed
// ==============================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

app.Run();
