namespace SIGA.Application.DTOs.Compras;

/// <summary>Recepción embebida en la respuesta detalle de la OC.</summary>
public class RecepcionComprasResponse
{
    public int Id { get; set; }
    public string FechaRecepcion { get; set; } = "";
    public string? UsuarioNombre { get; set; }
    public string? Observaciones { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<RecepcionComprasItemResponse> Items { get; set; } = [];
}

public class RecepcionComprasItemResponse
{
    public int PedidoItemId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public int Cantidad { get; set; }
    public string? Lote { get; set; }
    public string? FechaVencimiento { get; set; }
    public string? Observaciones { get; set; }
}
