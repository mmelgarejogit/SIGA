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
    public DbSet<DatosFacturacion> DatosFacturacion => Set<DatosFacturacion>();
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
    public DbSet<CategoriaProducto> CategoriasProducto => Set<CategoriaProducto>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Modelo> Modelos => Set<Modelo>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<PedidoProveedor> PedidosProveedor => Set<PedidoProveedor>();
    public DbSet<PedidoProveedorItem> PedidosProveedorItems => Set<PedidoProveedorItem>();
    public DbSet<DevolucionProveedor> DevolucionesProveedor => Set<DevolucionProveedor>();
    public DbSet<ConfiguracionNegocio> ConfiguracionNegocio => Set<ConfiguracionNegocio>();
    public DbSet<EstadoConfig> EstadosConfig => Set<EstadoConfig>();
    public DbSet<CategoriaGasto> CategoriasGasto => Set<CategoriaGasto>();
    public DbSet<Egreso> Egresos => Set<Egreso>();
    public DbSet<FacturaCompra> FacturasCompra => Set<FacturaCompra>();
    public DbSet<Honorario> Honorarios => Set<Honorario>();
    public DbSet<GastoGeneral> GastosGenerales => Set<GastoGeneral>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaLinea> VentaLineas => Set<VentaLinea>();
    public DbSet<Cobro> Cobros => Set<Cobro>();
    public DbSet<FacturaVenta> FacturasVenta => Set<FacturaVenta>();
    public DbSet<MovimientoCaja> MovimientosCaja => Set<MovimientoCaja>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
