namespace SIGA.Application.DTOs.Ventas;

public class VentaLineaDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = "";
    public int? ProductoId { get; set; }
    public int? ServicioId { get; set; }
    public string Descripcion { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public string CategoriaFiscal { get; set; } = "";
    public decimal Subtotal { get; set; }
}

public class CobroLineaDto
{
    public int Id { get; set; }
    public string MetodoPago { get; set; } = "";
    public decimal Monto { get; set; }
}

public class CobroDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = "";
    public decimal MontoTotal { get; set; }
    public string Fecha { get; set; } = "";
    public bool Anulado { get; set; }
    public List<CobroLineaDto> Lineas { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class FacturaVentaDto
{
    public int Id { get; set; }
    public string NumeroFactura { get; set; } = "";
    public string Timbrado { get; set; } = "";
    public string Establecimiento { get; set; } = "";
    public decimal MontoExento { get; set; }
    public decimal MontoGravado5 { get; set; }
    public decimal MontoGravado10 { get; set; }
    public decimal Iva5 { get; set; }
    public decimal Iva10 { get; set; }
    public decimal Total { get; set; }
    public string FechaEmision { get; set; } = "";
    public string? Observaciones { get; set; }
}

public class ComprobanteDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = "";
    public string Estado { get; set; } = "";
    public string? MotivoAnulacion { get; set; }
    public string FechaEmision { get; set; } = "";
    public string? FechaAnulacion { get; set; }
}

public class VentaDto
{
    public int Id { get; set; }
    public string NumeroComprobante { get; set; } = "";
    public int SucursalId { get; set; }
    public string? SucursalNombre { get; set; }
    public int? ClienteId { get; set; }
    public string ClienteNombre { get; set; } = "";
    public int? RecetaId { get; set; }
    public string Estado { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string CondicionVenta { get; set; } = "";
    public string FechaVenta { get; set; } = "";
    public int ValidezDias { get; set; }
    public string? FechaConfirmacion { get; set; }
    public string? FechaComprobante { get; set; }
    public decimal MontoExento { get; set; }
    public decimal MontoGravado5 { get; set; }
    public decimal MontoGravado10 { get; set; }
    public decimal Total { get; set; }
    public decimal MontoSeña { get; set; }
    public decimal TotalCobrado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string? Observaciones { get; set; }
    public List<VentaLineaDto> Lineas { get; set; } = new();
    public List<CobroDto> Cobros { get; set; } = new();
    public FacturaVentaDto? Factura { get; set; }
    public ComprobanteDto? Comprobante { get; set; }
    public TrabajoPedidoDto? TrabajoPedido { get; set; }
    public List<DevolucionDto> Devoluciones { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
