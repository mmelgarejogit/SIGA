namespace SIGA.Domain.Entities;

public class EgresoPago
{
    public int Id { get; set; }
    public int EgresoId { get; set; }
    public Egreso Egreso { get; set; } = null!;
    public DateOnly FechaPago { get; set; }
    public MetodoPago MetodoPago { get; set; }
    public string? NumeroComprobante { get; set; }
    public string? Observaciones { get; set; }
    public int RegistradoPorUserId { get; set; }
    public User? RegistradoPorUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}