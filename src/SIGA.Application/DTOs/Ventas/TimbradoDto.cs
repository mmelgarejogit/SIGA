namespace SIGA.Application.DTOs.Ventas;

public class TimbradoDto
{
    public int Id { get; set; }
    public int SucursalId { get; set; }
    public string? SucursalNombre { get; set; }
    public string Tipo { get; set; } = "Factura";
    public string NumeroTimbrado { get; set; } = "";
    public string Establecimiento { get; set; } = "";
    public string PuntoExpedicion { get; set; } = "";
    public int UltimoNumero { get; set; }
    public int ProximoNumero => UltimoNumero + 1;
    public string NumeroCompletoPreview => $"{Establecimiento}-{PuntoExpedicion}-{ProximoNumero:D7}";
    public int NumeroDesde { get; set; }
    public int? NumeroHasta { get; set; }
    public DateOnly FechaInicioVigencia { get; set; }
    public DateOnly FechaFinVigencia { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTimbradoRequest
{
    public int SucursalId { get; set; }
    /// <summary>"Factura" (default) o "NotaCredito".</summary>
    public string Tipo { get; set; } = "Factura";
    public string NumeroTimbrado { get; set; } = "";
    public string Establecimiento { get; set; } = "";
    public string PuntoExpedicion { get; set; } = "";
    public int NumeroDesde { get; set; } = 1;
    public int? NumeroHasta { get; set; }
    public DateOnly FechaInicioVigencia { get; set; }
    public DateOnly FechaFinVigencia { get; set; }
}

public class UpdateTimbradoRequest
{
    public int SucursalId { get; set; }
    public string Tipo { get; set; } = "Factura";
    public string NumeroTimbrado { get; set; } = "";
    public string Establecimiento { get; set; } = "";
    public string PuntoExpedicion { get; set; } = "";
    public int NumeroDesde { get; set; } = 1;
    public int? NumeroHasta { get; set; }
    public DateOnly FechaInicioVigencia { get; set; }
    public DateOnly FechaFinVigencia { get; set; }
    public bool IsActive { get; set; }
}