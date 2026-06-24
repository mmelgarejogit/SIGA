namespace SIGA.Application.DTOs.Ventas;

public class TimbradoDto
{
    public int Id { get; set; }
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
    public string NumeroTimbrado { get; set; } = "";
    public string Establecimiento { get; set; } = "";
    public string PuntoExpedicion { get; set; } = "";
    public int NumeroDesde { get; set; } = 1;
    public int? NumeroHasta { get; set; }
    public DateOnly FechaInicioVigencia { get; set; }
    public DateOnly FechaFinVigencia { get; set; }
    public bool IsActive { get; set; }
}