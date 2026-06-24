namespace SIGA.Application.DTOs.Ventas;

public class DevolucionLineaDto
{
    public int Id { get; set; }
    public int ProductoDevueltoId { get; set; }
    public string ProductoDevueltoNombre { get; set; } = "";
    public int CantidadDevuelta { get; set; }
    public int? ProductoNuevoId { get; set; }
    public string? ProductoNuevoNombre { get; set; }
    public int? CantidadNueva { get; set; }
}

public class DevolucionDto
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public string NumeroComprobante { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Estado { get; set; } = "";
    public string Motivo { get; set; } = "";
    public string SolicitadoPorNombre { get; set; } = "";
    public string? ConfirmadoPorNombre { get; set; }
    public string? ObservacionesRevision { get; set; }
    public string? FechaRevision { get; set; }
    public List<DevolucionLineaDto> Lineas { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class DevolucionLineaRequest
{
    public int ProductoDevueltoId { get; set; }
    public int CantidadDevuelta { get; set; }
    public int? ProductoNuevoId { get; set; }
    public int? CantidadNueva { get; set; }
}

public class SolicitarDevolucionRequest
{
    public string Tipo { get; set; } = "Devolucion";
    public string Motivo { get; set; } = null!;
    public List<DevolucionLineaRequest> Lineas { get; set; } = new();
}

public class GestionarDevolucionRequest
{
    public string Accion { get; set; } = null!;  // "Confirmar" | "Rechazar"
    public string? ObservacionesRevision { get; set; }
}
