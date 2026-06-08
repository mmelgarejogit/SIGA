namespace SIGA.Domain.Entities;

public enum EstadoSesionCaja { Abierta, Cerrada, PendienteAprobacion }

public class SesionCaja
{
    public int Id { get; set; }
    public EstadoSesionCaja Estado { get; set; }
    public decimal MontoInicial { get; set; }

    public int AbiertaPorId { get; set; }
    public User? AbiertaPor { get; set; }
    public DateTime FechaApertura { get; set; }

    public int? CerradaPorId { get; set; }
    public User? CerradaPor { get; set; }
    public DateTime? FechaCierre { get; set; }

    public decimal? EfectivoContado { get; set; }
    public decimal? EfectivoEsperado { get; set; }
    public decimal? Diferencia { get; set; }
    public string? ObservacionCierre { get; set; }

    // Aprobación de cierre
    public int? AprobadoPorId { get; set; }
    public User? AprobadoPor { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string? MotivoRechazo { get; set; }

    public List<MovimientoCaja> Movimientos { get; set; } = new();
}
