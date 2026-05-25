namespace SIGA.Application.DTOs.Compras;

/// <summary>Factura habilitada para registrar recepción de mercadería.</summary>
public class FacturaDisponibleResponse
{
    public int Id { get; set; }
    public string NroFactura { get; set; } = "";
    public string FechaEmision { get; set; } = "";
    public int PedidoProveedorId { get; set; }
    public string EstadoOC { get; set; } = "";
    public int ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = "";
    /// <summary>Cantidad de ítems con cantidad pendiente de recepción.</summary>
    public int ItemsPendientes { get; set; }
}
