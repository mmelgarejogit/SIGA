namespace SIGA.Application.DTOs.Ventas;

public class AnularVentaRequest
{
    public int VentaId { get; set; }
    public string Motivo { get; set; } = null!;
}
