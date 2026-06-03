namespace SIGA.Application.DTOs.Productos;

public class PedidoProveedorItemResponse
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

public class PedidoProveedorResponse
{
    public int Id { get; set; }
    public int ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = "";
    public string Estado { get; set; } = "";
    public int? EstadoId { get; set; }
    public string EstadoColor { get; set; } = "#6B7280";
    public string? Observaciones { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public IEnumerable<PedidoProveedorItemResponse> Items { get; set; } = [];
}
