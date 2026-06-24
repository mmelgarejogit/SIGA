namespace SIGA.Application.DTOs.Egresos;

public class RegistrarPagoRequest
{
    public string MetodoPago { get; set; } = "";
    public string FechaPago { get; set; } = "";
    public string? NroComprobante { get; set; }
    public string? Observaciones { get; set; }
    public bool EsExterno { get; set; }
    public string? MotivoExterno { get; set; }
}
