namespace SIGA.Domain.Entities;

public enum TipoMovimiento { Ingreso, Egreso }

public enum OrigenMovimiento
{
    Compra,
    Venta,
    DevolucionVenta,
    DevolucionProveedor,
    Transferencia,
    AjusteManual,
    CorreccionConteo
}

public class MovimientoInventario
{
    public Guid Id { get; set; }
    public Guid ProductoVarianteId { get; set; }
    public ProductoVariante ProductoVariante { get; set; } = null!;
    public Guid SucursalId { get; set; }
    public Sucursal Sucursal { get; set; } = null!;
    public TipoMovimiento Tipo { get; set; }
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public int UsuarioId { get; set; }
    public User Usuario { get; set; } = null!;
    public OrigenMovimiento OrigenTipo { get; set; }
    public Guid? ReferenciaId { get; set; }
    public Guid? TipoAjusteId { get; set; }
    public TipoAjuste? TipoAjuste { get; set; }
}
