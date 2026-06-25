namespace SIGA.Application.DTOs.Reportes;

public class ReporteVentasDto
{
    public string Desde { get; set; } = "";
    public string Hasta { get; set; } = "";
    public string Agrupacion { get; set; } = "";

    // ── KPIs ──────────────────────────────────────────────────────────────────
    /// <summary>Σ Total de ventas con comprobante emitido en el rango (por FechaComprobante).</summary>
    public decimal TotalFacturado { get; set; }
    /// <summary>Σ cobros no anulados en el rango (por Fecha del cobro) = caja real.</summary>
    public decimal TotalCobrado { get; set; }
    public int CantidadVentas { get; set; }
    public decimal TicketPromedio { get; set; }
    /// <summary>Cuentas por cobrar: saldo de las ventas emitidas en el rango aún no saldadas.</summary>
    public decimal SaldoPendiente { get; set; }
    public int CantidadPresupuestos { get; set; }
    /// <summary>Porcentaje (0–100) de ventas creadas en el rango que llegaron a ComprobanteEmitido.</summary>
    public decimal TasaConversion { get; set; }

    // ── Desgloses ─────────────────────────────────────────────────────────────
    public List<SeriePuntoDto> SerieTemporal { get; set; } = new();
    public List<MetodoPagoMontoDto> PorMetodoPago { get; set; } = new();
    public List<CondicionMontoDto> PorCondicion { get; set; } = new();
    public List<CategoriaFiscalMontoDto> PorCategoriaFiscal { get; set; } = new();
    public List<RankingItemDto> TopProductos { get; set; } = new();
    public List<RankingItemDto> TopServicios { get; set; } = new();
    public List<CajeroMontoDto> PorCajero { get; set; } = new();
}

public class SeriePuntoDto
{
    public string Periodo { get; set; } = "";
    public decimal Facturado { get; set; }
    public decimal Cobrado { get; set; }
}

public class MetodoPagoMontoDto
{
    public string Metodo { get; set; } = "";
    public decimal Monto { get; set; }
    public decimal Porcentaje { get; set; }
}

public class CondicionMontoDto
{
    public string Condicion { get; set; } = "";
    public decimal Monto { get; set; }
    public int Cantidad { get; set; }
}

public class CategoriaFiscalMontoDto
{
    public string Categoria { get; set; } = "";
    public decimal Monto { get; set; }
}

public class RankingItemDto
{
    public string Nombre { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}

public class CajeroMontoDto
{
    public string Nombre { get; set; } = "";
    public decimal Monto { get; set; }
    public int Cantidad { get; set; }
}
