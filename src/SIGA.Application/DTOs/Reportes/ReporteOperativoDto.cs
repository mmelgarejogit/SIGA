namespace SIGA.Application.DTOs.Reportes;

/// <summary>
/// Resultado genérico de un reporte operativo: filas de la página + metadatos de paginación +
/// totales agregados sobre TODO el conjunto filtrado (no solo la página). <c>Totales</c> es un
/// diccionario clave→monto (p. ej. "total", "cobrado", "ingresos") para que la UI/exportador lo rinda.
/// </summary>
public class ReporteOperativoDto<TRow>
{
    public List<TRow> Rows { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public Dictionary<string, decimal> Totales { get; set; } = [];
}

public class ReporteVentaRow
{
    public DateOnly Fecha { get; set; }
    public string NumeroComprobante { get; set; } = "";
    public string Cliente { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Condicion { get; set; } = "";
    public string Estado { get; set; } = "";
    public decimal Total { get; set; }
    public decimal Cobrado { get; set; }
    public decimal Saldo => Total - Cobrado;
    public string Sucursal { get; set; } = "";
    public string Vendedor { get; set; } = "";
}

public class ReporteCompraRow
{
    public DateOnly Fecha { get; set; }
    public string NroFactura { get; set; } = "";
    public string Proveedor { get; set; } = "";
    public string Condicion { get; set; } = "";
    public string Estado { get; set; } = "";
    public string MetodoPago { get; set; } = "";
    public decimal MontoTotal { get; set; }
    public string Sucursal { get; set; } = "";
    public string RegistradoPor { get; set; } = "";
}

public class ReporteMovInventarioRow
{
    public DateTime Fecha { get; set; }
    public string Producto { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Tipo { get; set; } = "";
    public int Cantidad { get; set; }
    public string Motivo { get; set; } = "";
    public string Estado { get; set; } = "";
    public string CreadoPor { get; set; } = "";
    public string Sucursal { get; set; } = "";
}

public class ReporteMovCajaRow
{
    public DateOnly Fecha { get; set; }
    public string Tipo { get; set; } = "";
    public string Concepto { get; set; } = "";
    public string MetodoPago { get; set; } = "";
    public decimal Monto { get; set; }
    public string RegistradoPor { get; set; } = "";
    public string Sucursal { get; set; } = "";
    public string Referencia { get; set; } = "";
}
