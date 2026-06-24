namespace SIGA.Application.DTOs.Compras;

/// <summary>Fila de la lista standalone de recepciones.</summary>
public class RecepcionListResponse
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

    public int CantidadItems { get; set; }
    public int CantidadTotal { get; set; }

    public string? Observaciones { get; set; }
}
