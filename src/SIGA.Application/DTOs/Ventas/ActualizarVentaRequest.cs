namespace SIGA.Application.DTOs.Ventas;

/// <summary>
/// Actualiza una venta en estado Borrador (presupuesto) antes de confirmarla.
/// No cambia cliente ni tipo: solo condición de pago, fecha, observaciones, las
/// líneas facturables y —para trabajos a pedido— el laboratorio asignado.
/// </summary>
public class ActualizarVentaRequest
{
    public string CondicionVenta { get; set; } = "Contado";
    public string FechaVenta { get; set; } = null!;
    public string? Observaciones { get; set; }
    public List<AgregarLineaRequest> Lineas { get; set; } = new();

    /// <summary>Laboratorio para el trabajo a pedido. Si es null, se conserva el actual.</summary>
    public int? LaboratorioProveedorId { get; set; }
}
