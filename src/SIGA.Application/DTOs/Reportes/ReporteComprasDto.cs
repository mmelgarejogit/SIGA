namespace SIGA.Application.DTOs.Reportes;

public class ReporteComprasDto
{
    public string Desde { get; set; } = "";
    public string Hasta { get; set; } = "";
    public string Agrupacion { get; set; } = "";

    // ── KPIs del período ────────────────────────────────────────────────────────
    /// <summary>Órdenes de compra con FechaOrden dentro del rango.</summary>
    public int OrdenesCompra { get; set; }
    /// <summary>Σ total de facturas de compra del rango (excluye anuladas/rechazadas).</summary>
    public decimal MontoFacturado { get; set; }
    public decimal Iva { get; set; }
    /// <summary>Σ total de facturas del rango aún sin pagar (Pendiente/Aprobado).</summary>
    public decimal PendientePago { get; set; }
    public int Recepciones { get; set; }

    // ── Desgloses ─────────────────────────────────────────────────────────────
    public List<SeriePuntoComprasDto> SerieTemporal { get; set; } = new();
    public List<EstadoOcDto> PorEstadoOc { get; set; } = new();
    public List<ProveedorComprasDto> PorProveedor { get; set; } = new();
}

public class SeriePuntoComprasDto
{
    public string Periodo { get; set; } = "";
    public decimal Monto { get; set; }
}

public class EstadoOcDto
{
    public string Estado { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal Porcentaje { get; set; }
}

public class ProveedorComprasDto
{
    public string Nombre { get; set; } = "";
    public int Facturas { get; set; }
    public decimal Monto { get; set; }
}
