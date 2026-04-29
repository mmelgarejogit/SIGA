namespace SIGA.Domain.Entities;

public class Receta
{
    public int Id { get; set; }

    public int ConsultaClinicaId { get; set; }
    public ConsultaClinica ConsultaClinica { get; set; } = null!;

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

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
