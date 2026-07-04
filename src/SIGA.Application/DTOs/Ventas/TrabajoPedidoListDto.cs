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
    public string ClienteNombre { get; set; } = "";
    public string TipoLenteNombre { get; set; } = "";
    public List<string> Tratamientos { get; set; } = new();
    public string LaboratorioNombre { get; set; } = "";
    public string Estado { get; set; } = "";
    public string? ObservacionAprobacion { get; set; }
    public string? AprobadoPorNombre { get; set; }
    public string? FechaEnvio { get; set; }
    public string? FechaEstimadaEntrega { get; set; }
    public string? MedioEnvio { get; set; }
    public string? FechaRecepcion { get; set; }
    public string? Observacion { get; set; }
    public FacturaLaboratorioDto? Factura { get; set; }
    public DateTime CreatedAt { get; set; }

    // ── Referencia de la venta (datos que el laboratorio necesita para fabricar) ──
    // El lente se describe por TipoLenteNombre (diseño) + Tratamientos.
    public string? ArmazonNombre { get; set; }
    public bool ArmazonDelCliente { get; set; }
    public RecetaRefDto? Receta { get; set; }
}

/// <summary>Prescripción de la receta asociada a la venta — referencia de solo lectura para el laboratorio.</summary>
public class RecetaRefDto
{
    public string FechaEmision { get; set; } = "";

    public decimal? OdEsferico { get; set; }
    public decimal? OdCilindro { get; set; }
    public int? OdEje { get; set; }
    public decimal? OdAdicion { get; set; }

    public decimal? OiEsferico { get; set; }
    public decimal? OiCilindro { get; set; }
    public int? OiEje { get; set; }
    public decimal? OiAdicion { get; set; }

    public decimal? DistanciaInterpupilar { get; set; }
    public string? Observaciones { get; set; }
}

public class GestionarTrabajoPedidoRequest
{
    public string Accion { get; set; } = null!;   // "Aprobar" | "Rechazar"
    public string? Observacion { get; set; }
}

/// <summary>Datos del envío al laboratorio: compromiso de entrega y medio por el que se comunicó.</summary>
public class RegistrarEnvioRequest
{
    public string? FechaEstimadaEntrega { get; set; }   // "yyyy-MM-dd"
    public string? MedioEnvio { get; set; }             // MedioEnvioLaboratorio (enum por nombre)
}

public class EmitirFacturaLaboratorioRequest
{
    public string NumeroFactura { get; set; } = null!;
    public string? Timbrado { get; set; }
    public string FechaEmision { get; set; } = null!;
    public decimal Monto { get; set; }
    public string? Observaciones { get; set; }
}
