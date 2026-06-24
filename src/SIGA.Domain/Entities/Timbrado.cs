namespace SIGA.Domain.Entities;

public class Timbrado
{
    public int Id { get; set; }
    public string NumeroTimbrado { get; set; } = null!;
    public string Establecimiento { get; set; } = null!;
    public string PuntoExpedicion { get; set; } = null!;
    public int UltimoNumero { get; set; }
    public int NumeroDesde { get; set; } = 1;
    public int? NumeroHasta { get; set; }
    public DateOnly FechaInicioVigencia { get; set; }
    public DateOnly FechaFinVigencia { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<FacturaVenta> FacturasVenta { get; set; } = new List<FacturaVenta>();
}