namespace SIGA.Domain.Entities;

public enum TipoDocumentoFiscal
{
    Factura     = 1,
    NotaCredito = 2,
}

public class Timbrado
{
    public int Id { get; set; }
    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }

    /// <summary>Tipo de documento que numera este timbrado. Factura y Nota de Crédito tienen series separadas.</summary>
    public TipoDocumentoFiscal Tipo { get; set; } = TipoDocumentoFiscal.Factura;

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