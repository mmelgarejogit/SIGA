namespace SIGA.Application.DTOs.Compras;

public class PedidoResponse
{
    public int Id { get; set; }
    public int ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = "";
    public string Estado { get; set; } = "";
    public string? Observaciones { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public IEnumerable<PedidoItemResponse> Items { get; set; } = [];
    public IEnumerable<DevolucionResponse> Devoluciones { get; set; } = [];
}

public class PedidoItemResponse
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public int Cantidad { get; set; }
    public int CantidadRecibida { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total => Cantidad * PrecioUnitario;
}

public class DevolucionResponse
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public int Cantidad { get; set; }
    public string Motivo { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
