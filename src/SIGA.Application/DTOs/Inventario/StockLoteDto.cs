namespace SIGA.Application.DTOs.Inventario;

public class StockLoteDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public string? ProductoSku { get; set; }
    public string? MarcaNombre { get; set; }
    public string? ModeloNombre { get; set; }
    public string Lote { get; set; } = "";
    public string? FechaVencimiento { get; set; }
    public int CantidadInicial { get; set; }
    public string FechaIngreso { get; set; } = "";
    public int RecepcionItemId { get; set; }
}

// ── Registrar conteo (por producto) ──────────────────────────────────────────

public class ConteoItemRequest
{
    public int ProductoId { get; set; }
    public int CantidadFisica { get; set; }
}

public class RegistrarConteoRequest
{
    public List<ConteoItemRequest> Items { get; set; } = [];
    public string? Observaciones { get; set; }
}

// ── Respuestas de conteo ──────────────────────────────────────────────────────

public class ConteoInventarioDto
{
    public int Id { get; set; }
    public string CreadoPorNombre { get; set; } = "";
    public string FechaConteo { get; set; } = "";
    public string Estado { get; set; } = "";
    public string? Observaciones { get; set; }
    public string? AprobadoPorNombre { get; set; }
    public string? FechaAprobacion { get; set; }
    public string? ObservacionesAprobacion { get; set; }
    public int TotalLineas { get; set; }
    public int LineasConDiferencia { get; set; }
}

public class ConteoInventarioDetalleDto : ConteoInventarioDto
{
    public List<ConteoLineaDto> Lineas { get; set; } = [];
}

public class ConteoLineaDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public string? ProductoSku { get; set; }
    public string? ProductoCategoria { get; set; }
    public int CantidadSistema { get; set; }
    public int CantidadFisica { get; set; }
    public int Diferencia { get; set; }
}

// ── Aprobar / rechazar conteo ─────────────────────────────────────────────────

public class GestionarConteoRequest
{
    public string Accion { get; set; } = ""; // Aprobado | Rechazado
    public string? Observaciones { get; set; }
}
