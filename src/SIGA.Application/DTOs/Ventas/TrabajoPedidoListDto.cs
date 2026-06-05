namespace SIGA.Application.DTOs.Ventas;

public class FacturaLaboratorioDto
{
    public int Id { get; set; }
    public string NumeroFactura { get; set; } = "";
    public string? Timbrado { get; set; }
    public string FechaEmision { get; set; } = "";
    public decimal Monto { get; set; }
    public string? Observaciones { get; set; }
    public string EmitidoPorNombre { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class TrabajoPedidoListDto
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public string NumeroComprobante { get; set; } = "";
    public string PacienteNombre { get; set; } = "";
    public string TipoLenteNombre { get; set; } = "";
    public List<string> Tratamientos { get; set; } = new();
    public string LaboratorioNombre { get; set; } = "";
    public string Estado { get; set; } = "";
    public string? ObservacionAprobacion { get; set; }
    public string? AprobadoPorNombre { get; set; }
    public string? FechaEnvio { get; set; }
    public string? FechaRecepcion { get; set; }
    public string? Observacion { get; set; }
    public FacturaLaboratorioDto? Factura { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GestionarTrabajoPedidoRequest
{
    public string Accion { get; set; } = null!;   // "Aprobar" | "Rechazar"
    public string? Observacion { get; set; }
}

public class EmitirFacturaLaboratorioRequest
{
    public string NumeroFactura { get; set; } = null!;
    public string? Timbrado { get; set; }
    public string FechaEmision { get; set; } = null!;
    public decimal Monto { get; set; }
    public string? Observaciones { get; set; }
}
