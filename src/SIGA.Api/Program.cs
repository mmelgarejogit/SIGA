using Microsoft.OpenApi;
using SIGA.Infrastructure; 

var builder = WebApplication.CreateBuilder(args);

// ==============================
// 1️⃣ Servicios
builder.Services.AddInfrastructure(builder.Configuration);
// ==============================

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

app.UseAuthorization();

app.MapControllers();

app.Run();