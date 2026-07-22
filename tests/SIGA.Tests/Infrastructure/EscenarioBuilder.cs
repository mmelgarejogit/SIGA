using Microsoft.EntityFrameworkCore;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Tests.Infrastructure;

/// <summary>
/// Arma los datos mínimos que necesita un test de ventas: sucursal, usuario, producto con
/// stock y la venta en el estado que corresponda. Existe para que cada test diga en dos
/// líneas de qué parte, y el "porqué" del test no quede sepultado bajo treinta líneas de
/// alta de entidades.
/// </summary>
public sealed class EscenarioBuilder(AppDbContext db)
{
    private int _secuencia;

    /// <summary>La sucursal por defecto que dejan las migraciones (Casa Central).</summary>
    public async Task<Sucursal> SucursalPorDefectoAsync() =>
        await db.Sucursales.FirstAsync(s => s.Codigo == "CC");

    public async Task<Sucursal> CrearSucursalAsync(string codigo, string? nombre = null)
    {
        var sucursal = new Sucursal
        {
            Nombre = nombre ?? $"Sucursal {codigo}",
            Codigo = codigo,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        db.Sucursales.Add(sucursal);
        await db.SaveChangesAsync();
        return sucursal;
    }

    public async Task<User> CrearUsuarioAsync(string nombre = "Operador", int? sucursalId = null)
    {
        var n = ++_secuencia;
        var person = new Person
        {
            CI = $"TEST{n:D6}",
            FirstName = nombre,
            LastName = $"Prueba{n}",
            BirthDate = new DateOnly(1990, 1, 1),
            Email = $"operador{n}@test.local",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Persons.Add(person);
        await db.SaveChangesAsync();

        var user = new User
        {
            PersonId = person.Id,
            SucursalId = sucursalId,
            PasswordHash = "no-usado-en-tests",
            IsActive = true,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Producto con stock inicial. El stock no es una columna: se deriva de los
    /// MovimientoStock aprobados a través de la vista vw_stock_actual, así que "dar stock"
    /// significa registrar una entrada.
    /// </summary>
    public async Task<Producto> CrearProductoConStockAsync(
        int sucursalId, int stockInicial, decimal precioVenta = 100_000, string nombre = "Armazón Vulk")
    {
        var n = ++_secuencia;
        var categoria = new CategoriaProducto
        {
            Nombre = $"Categoría {n}",
            Tipo = TipoCategoriaProducto.Armazon,
            Margen = 100,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        db.CategoriasProducto.Add(categoria);
        await db.SaveChangesAsync();

        var producto = new Producto
        {
            Nombre = $"{nombre} {n}",
            Categoria = categoria.Nombre,
            CategoriaProductoId = categoria.Id,
            Sku = $"SKU{n:D6}",
            PrecioCosto = precioVenta / 2,
            PrecioVenta = precioVenta,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Productos.Add(producto);
        await db.SaveChangesAsync();

        if (stockInicial > 0)
            await DarStockAsync(producto.Id, sucursalId, stockInicial);

        return producto;
    }

    public async Task DarStockAsync(int productoId, int sucursalId, int cantidad)
    {
        db.MovimientosStock.Add(new MovimientoStock
        {
            ProductoId = productoId,
            SucursalId = sucursalId,
            Tipo = TipoMovimientoStock.Entrada,
            Cantidad = cantidad,
            Motivo = "Carga inicial del escenario de prueba",
            FechaMovimiento = DateTime.UtcNow,
            Estado = EstadoMovimientoStock.Aprobado,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    public async Task<int> StockDeAsync(int productoId, int sucursalId)
    {
        var fila = await db.StockActual
            .FirstOrDefaultAsync(s => s.ProductoId == productoId && s.SucursalId == sucursalId);
        return fila?.StockActual ?? 0;
    }

    /// <summary>
    /// Venta de contado en estado ListaParaCobrar (el paso previo a emitir el documento
    /// fiscal), con una línea de producto.
    /// </summary>
    public async Task<Venta> CrearVentaListaParaCobrarAsync(
        int sucursalId, int vendedorId, Producto producto, int cantidad, decimal? precioUnitario = null)
    {
        var n = ++_secuencia;
        var precio = precioUnitario ?? producto.PrecioVenta;
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var venta = new Venta
        {
            NumeroComprobante = $"TEST-{n:D5}",
            SucursalId = sucursalId,
            VendedorId = vendedorId,
            Estado = EstadoVenta.ListaParaCobrar,
            Tipo = TipoVenta.Directa,
            CondicionVenta = CondicionVenta.Contado,
            FechaVenta = hoy,
            FechaConfirmacion = hoy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        venta.Lineas.Add(new VentaLinea
        {
            Tipo = TipoLineaVenta.Producto,
            ProductoId = producto.Id,
            Descripcion = producto.Nombre,
            Cantidad = cantidad,
            PrecioUnitario = precio,
            CategoriaFiscal = CategoriaFiscal.Gravado10,
        });

        db.Ventas.Add(venta);
        await db.SaveChangesAsync();
        return venta;
    }

    public async Task<SesionCaja> AbrirCajaAsync(int sucursalId, int usuarioId, decimal montoInicial = 500_000)
    {
        var sesion = new SesionCaja
        {
            SucursalId = sucursalId,
            Estado = EstadoSesionCaja.Abierta,
            MontoInicial = montoInicial,
            AbiertaPorId = usuarioId,
            FechaApertura = DateTime.UtcNow,
        };
        db.SesionesCaja.Add(sesion);
        await db.SaveChangesAsync();
        return sesion;
    }
}
