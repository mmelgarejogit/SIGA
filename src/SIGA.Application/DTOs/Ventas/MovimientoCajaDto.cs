namespace SIGA.Application.DTOs.Ventas;

public class MovimientoCajaDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = "";
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = "";
    public string MetodoPago { get; set; } = "";
    public int? VentaId { get; set; }
    public int? EgresoId { get; set; }
    public int? SesionCajaId { get; set; }
    public string? RegistradoPorNombre { get; set; }
    public string Fecha { get; set; } = "";
    public string? Referencia { get; set; }
    public DateTime CreatedAt { get; set; }
}
