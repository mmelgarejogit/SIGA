namespace SIGA.Domain.Entities;

public class RecepcionMercaderia
{
    public int Id { get; set; }

    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }

    public int PedidoProveedorId { get; set; }
    public PedidoProveedor PedidoProveedor { get; set; } = null!;

    /// <summary>Factura vinculada — el origen de la recepción es siempre una factura con OC.</summary>
    public int? FacturaCompraId { get; set; }
    public FacturaCompra? FacturaCompra { get; set; }

    /// <summary>Fecha en la que se recibió físicamente la mercadería (la define el usuario).</summary>
    public DateOnly FechaRecepcion { get; set; }

    /// <summary>Usuario que registró la recepción.</summary>
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string? Observaciones { get; set; }

    /// <summary>Timestamp de creación del registro en el sistema.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RecepcionMercaderiaItem> Items { get; set; } = [];
}
