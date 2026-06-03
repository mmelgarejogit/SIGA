namespace SIGA.Application.DTOs.Stock;

public class TransferenciaLineaResponse
{
    public Guid Id { get; set; }
    public Guid ProductoVarianteId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public string? Sku { get; set; }
    public string? Color { get; set; }
    public string? Talle { get; set; }
    public int Cantidad { get; set; }
}

public class TransferenciaResponse
{
    public Guid Id { get; set; }
    public Guid SucursalOrigenId { get; set; }
    public string SucursalOrigenNombre { get; set; } = "";
    public Guid SucursalDestinoId { get; set; }
    public string SucursalDestinoNombre { get; set; } = "";
    public string Estado { get; set; } = "";
    public int SolicitadoPorId { get; set; }
    public string SolicitadoPorNombre { get; set; } = "";
    public int? AprobadoPorId { get; set; }
    public string? AprobadoPorNombre { get; set; }
    public string? Observacion { get; set; }
    public string? MotivoRechazo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public List<TransferenciaLineaResponse> Lineas { get; set; } = [];
}

public class TransferenciaLineaRequest
{
    public Guid ProductoVarianteId { get; set; }
    public int Cantidad { get; set; }
}

public class CreateTransferenciaRequest
{
    public Guid SucursalOrigenId { get; set; }
    public Guid SucursalDestinoId { get; set; }
    public string? Observacion { get; set; }
    public List<TransferenciaLineaRequest> Lineas { get; set; } = [];
}

public class ResolverTransferenciaRequest
{
    public string Accion { get; set; } = "";  // "Aprobar" | "Rechazar"
    public string? MotivoRechazo { get; set; }
}
