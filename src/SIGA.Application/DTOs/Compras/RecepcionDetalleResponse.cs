namespace SIGA.Application.DTOs.Compras;

public class RecepcionDetalleResponse
{
    public int Id { get; set; }
    public string FechaRecepcion { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public int PedidoProveedorId { get; set; }
    public string EstadoOC { get; set; } = "";

    public int ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = "";

    public int? FacturaCompraId { get; set; }
    public string? NroFactura { get; set; }

    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = "";

    public string? Observaciones { get; set; }

    public IEnumerable<RecepcionComprasItemResponse> Items { get; set; } = [];
}
