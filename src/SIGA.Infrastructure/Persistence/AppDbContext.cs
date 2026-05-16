using Microsoft.EntityFrameworkCore;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Professional> Professionals => Set<Professional>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Especialidad> Especialidades => Set<Especialidad>();
    public DbSet<ProfesionalEspecialidad> ProfesionalEspecialidades => Set<ProfesionalEspecialidad>();
    public DbSet<HorarioProfesional> HorariosProfesional => Set<HorarioProfesional>();
    public DbSet<PausaHorario> PausasHorario => Set<PausaHorario>();
    public DbSet<BloqueoFecha> BloqueosFecha => Set<BloqueoFecha>();
    public DbSet<ConsultaClinica> ConsultasClinicas => Set<ConsultaClinica>();
    public DbSet<Receta> Recetas => Set<Receta>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<PedidoProveedor> PedidosProveedor => Set<PedidoProveedor>();
    public DbSet<PedidoProveedorItem> PedidosProveedorItems => Set<PedidoProveedorItem>();
    public DbSet<ConfiguracionNegocio> ConfiguracionNegocio => Set<ConfiguracionNegocio>();
    public DbSet<EstadoConfig> EstadosConfig => Set<EstadoConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
