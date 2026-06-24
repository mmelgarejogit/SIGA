namespace SIGA.Application.DTOs.Ventas;

public class SesionCajaDto
{
    public int Id { get; set; }
    public string Estado { get; set; } = "";
    public decimal MontoInicial { get; set; }
    public string AbiertaPorNombre { get; set; } = "";
    public DateTime FechaApertura { get; set; }
    public string? CerradaPorNombre { get; set; }
    public DateTime? FechaCierre { get; set; }

    // Arqueo (solo en cierre)
    public decimal? EfectivoContado { get; set; }
    public decimal? EfectivoEsperado { get; set; }
    public decimal? Diferencia { get; set; }
    public string? ObservacionCierre { get; set; }

    // Aprobación de cierre
    public string? AprobadoPorNombre { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string? MotivoRechazo { get; set; }

    // Resumen de movimientos
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal SaldoNeto { get; set; }
    public decimal EfectivoIngresos { get; set; }
    public decimal TarjetaTotal { get; set; }
    public decimal TransferenciaTotal { get; set; }
    public decimal ChequeTotal { get; set; }
    public int CantidadMovimientos { get; set; }

    public List<MovimientoCajaDto> Movimientos { get; set; } = new();
}

public class SesionCajaListDto
{
    public int Id { get; set; }
    public string Estado { get; set; } = "";
    public decimal MontoInicial { get; set; }
    public string AbiertaPorNombre { get; set; } = "";
    public DateTime FechaApertura { get; set; }
    public string? CerradaPorNombre { get; set; }
    public DateTime? FechaCierre { get; set; }
    public decimal? EfectivoContado { get; set; }
    public decimal? EfectivoEsperado { get; set; }
    public decimal? Diferencia { get; set; }
    public string? AprobadoPorNombre { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string? MotivoRechazo { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal SaldoNeto { get; set; }
}

public record RechazarCierreRequest
{
    public string Motivo { get; init; } = "";
}
