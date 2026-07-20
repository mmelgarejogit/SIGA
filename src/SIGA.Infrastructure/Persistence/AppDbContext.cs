using Microsoft.EntityFrameworkCore;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserContext? currentUser = null) : DbContext(options)
{
    // null = usuario global (admin) o contexto sin ICurrentUserContext (seeders, background
    // services, herramientas de diseño como `dotnet ef migrations add`) — el filtro es un no-op.
    private int? SucursalFiltro => currentUser?.SucursalId;

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
    public DbSet<EgresoFacturaLaboratorio> EgresosFacturaLaboratorio => Set<EgresoFacturaLaboratorio>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<ServicioTarifa> ServicioTarifas => Set<ServicioTarifa>();
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
    public DbSet<NotaCredito> NotasCredito => Set<NotaCredito>();
    public DbSet<MovimientoCaja> MovimientosCaja => Set<MovimientoCaja>();
    public DbSet<Timbrado> Timbrados => Set<Timbrado>();
    public DbSet<SesionCaja> SesionesCaja => Set<SesionCaja>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<TransferenciaStock> TransferenciasStock => Set<TransferenciaStock>();
    public DbSet<TransferenciaStockItem> TransferenciasStockItems => Set<TransferenciaStockItem>();
    public DbSet<NotificacionInterna> NotificacionesInternas => Set<NotificacionInterna>();
    public DbSet<NotificacionPreferencia> NotificacionesPreferencias => Set<NotificacionPreferencia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        AplicarFiltrosDeSucursal(modelBuilder);
    }

    // Aislamiento multi-sucursal: cada entidad con dimensión de sucursal queda restringida
    // a la sucursal del usuario actual en TODA query (list, GetById, Include, etc.), sin
    // depender de que cada servicio recuerde aplicar el guard a mano. Un usuario global
    // (SucursalFiltro == null) no queda restringido. No afecta INSERT/UPDATE/DELETE — eso
    // sigue resuelto por SucursalResolver.WriteBranchAsync en el momento de escribir.
    //
    // Excluidas a propósito:
    //   - User: SucursalId es la asignación del propio usuario, no una fila "propiedad" de
    //     esa sucursal — filtrarla rompería Include(x => x.RegistradoPor) desde entidades ya
    //     scopeadas y la gestión de usuarios de otras sucursales por parte de un admin.
    //   - NotificacionInterna: DestinatarioSucursalId es un campo de targeting de broadcast
    //     (null = todas), no de ownership — NotificacionInternaService.VisibleQuery() ya
    //     combina esa lógica con el permiso ver_todas_sucursales; un filtro de una sola
    //     columna no puede expresar lo mismo.
    private void AplicarFiltrosDeSucursal(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConsultaClinica>().HasQueryFilter(e => SucursalFiltro == null || e.SucursalId == SucursalFiltro);
        modelBuilder.Entity<ConteoInventario>().HasQueryFilter(e => SucursalFiltro == null || e.SucursalId == SucursalFiltro);
        modelBuilder.Entity<Egreso>().HasQueryFilter(e => SucursalFiltro == null || e.SucursalId == SucursalFiltro);
        modelBuilder.Entity<HorarioProfesional>().HasQueryFilter(e => SucursalFiltro == null || e.SucursalId == SucursalFiltro);
        modelBuilder.Entity<MovimientoCaja>().HasQueryFilter(e => SucursalFiltro == null || e.SucursalId == SucursalFiltro);
        modelBuilder.Entity<MovimientoStock>().HasQueryFilter(e => SucursalFiltro == null || e.SucursalId == SucursalFiltro);
        modelBuilder.Entity<PedidoProveedor>().HasQueryFilter(e => SucursalFiltro == null || e.SucursalId == SucursalFiltro);
        modelBuilder.Entity<RecepcionMercaderia>().HasQueryFilter(e => SucursalFiltro == null || e.SucursalId == SucursalFiltro);
        modelBuilder.Entity<StockActualView>().HasQueryFilter(e => SucursalFiltro == null || e.SucursalId == SucursalFiltro);
        modelBuilder.Entity<StockLote>().HasQueryFilter(e => SucursalFiltro == null || e.SucursalId == SucursalFiltro);
        modelBuilder.Entity<SesionCaja>().HasQueryFilter(e => SucursalFiltro == null || e.SucursalId == SucursalFiltro);
        modelBuilder.Entity<Timbrado>().HasQueryFilter(e => SucursalFiltro == null || e.SucursalId == SucursalFiltro);
        modelBuilder.Entity<Turno>().HasQueryFilter(e => SucursalFiltro == null || e.SucursalId == SucursalFiltro);
        modelBuilder.Entity<Venta>().HasQueryFilter(e => SucursalFiltro == null || e.SucursalId == SucursalFiltro);

        // TransferenciaStock no tiene SucursalId propio: pertenece a dos sucursales a la vez
        // (origen/destino), visible desde ambas puntas.
        modelBuilder.Entity<TransferenciaStock>().HasQueryFilter(e =>
            SucursalFiltro == null || e.SucursalOrigenId == SucursalFiltro || e.SucursalDestinoId == SucursalFiltro);
    }
}
