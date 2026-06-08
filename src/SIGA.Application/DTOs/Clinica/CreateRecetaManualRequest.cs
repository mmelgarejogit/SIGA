namespace SIGA.Application.DTOs.Clinica;

/// <summary>
/// Alta de una receta externa/de mostrador (sin consulta clínica), vinculada a un cliente.
/// </summary>
public class CreateRecetaManualRequest
{
    public int ClienteId { get; set; }
    public DateOnly FechaEmision { get; set; }
    public decimal? OdEsferico { get; set; }
    public decimal? OdCilindro { get; set; }
    public int? OdEje { get; set; }
    public decimal? OdAdicion { get; set; }
    public decimal? OiEsferico { get; set; }
    public decimal? OiCilindro { get; set; }
    public int? OiEje { get; set; }
    public decimal? OiAdicion { get; set; }
    public decimal? DistanciaInterpupilar { get; set; }
    public string? AvSinCorreccion { get; set; }
    public string? AvConCorreccion { get; set; }
    public string? Observaciones { get; set; }
}
