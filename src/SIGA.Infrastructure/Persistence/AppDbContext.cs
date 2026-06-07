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
    public DbSet<Cliente> Clientes => Set<Cliente>();
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
    public DbSet<ProductoStockConfig> ProductosStockConfig => Set<ProductoStockConfig>();
    public DbSet<StockActualView> StockActual => Set<StockActualView>();
    public DbSet<MotivoMovimiento> MotivosMovimiento => Set<MotivoMovimiento>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<ProveedorContacto> ProveedorContactos => Set<ProveedorContacto>();
    public DbSet<PedidoProveedor> PedidosProveedor => Set<PedidoProveedor>();
    public DbSet<PedidoProveedorItem> PedidosProveedorItems => Set<PedidoProveedorItem>();
    public DbSet<DevolucionProveedor> DevolucionesProveedor => Set<DevolucionProveedor>();
    public DbSet<RecepcionMercaderia> RecepcionesMercaderia => Set<RecepcionMercaderia>();
    public DbSet<RecepcionMercaderiaItem> RecepcionesMercaderiaItems => Set<RecepcionMercaderiaItem>();
    public DbSet<StockLote> StockLotes => Set<StockLote>();
    public DbSet<ConteoInventario> ConteosInventario => Set<ConteoInventario>();
    public DbSet<ConteoInventarioLinea> ConteoInventarioLineas => Set<ConteoInventarioLinea>();
    public DbSet<ConfiguracionNegocio> ConfiguracionNegocio => Set<ConfiguracionNegocio>();
    public DbSet<Departamento> Departamentos => Set<Departamento>();
    public DbSet<Ciudad> Ciudades => Set<Ciudad>();
    public DbSet<EstadoConfig> EstadosConfig => Set<EstadoConfig>();
    public DbSet<CargoEmpleado> CargosEmpleado => Set<CargoEmpleado>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<CategoriaGasto> CategoriasGasto => Set<CategoriaGasto>();
    public DbSet<Egreso> Egresos => Set<Egreso>();
    public DbSet<FacturaCompra> FacturasCompra => Set<FacturaCompra>();
    public DbSet<FacturaCompraItem> FacturaCompraItems => Set<FacturaCompraItem>();
    public DbSet<Honorario> Honorarios => Set<Honorario>();
    public DbSet<GastoGeneral> GastosGenerales => Set<GastoGeneral>();
    public DbSet<SalarioEmpleado> SalariosEmpleado => Set<SalarioEmpleado>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaLinea> VentaLineas => Set<VentaLinea>();
    public DbSet<Cobro> Cobros => Set<Cobro>();
    public DbSet<CobroLinea> CobroLineas => Set<CobroLinea>();
    public DbSet<Comprobante> Comprobantes => Set<Comprobante>();
    public DbSet<TipoLente> TiposLente => Set<TipoLente>();
    public DbSet<Tratamiento> Tratamientos => Set<Tratamiento>();
    public DbSet<TrabajoPedido> TrabajosPedido => Set<TrabajoPedido>();
    public DbSet<FacturaLaboratorio> FacturasLaboratorio => Set<FacturaLaboratorio>();
    public DbSet<Devolucion> Devoluciones => Set<Devolucion>();
    public DbSet<DevolucionLinea> DevolucionLineas => Set<DevolucionLinea>();
    public DbSet<FacturaVenta> FacturasVenta => Set<FacturaVenta>();
    public DbSet<MovimientoCaja> MovimientosCaja => Set<MovimientoCaja>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
