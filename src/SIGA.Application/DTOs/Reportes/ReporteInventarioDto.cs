namespace SIGA.Application.DTOs.Reportes;

public class ReporteInventarioDto
{
    public string Desde { get; set; } = "";
    public string Hasta { get; set; } = "";
    public string Agrupacion { get; set; } = "";

    // ── KPIs (snapshot del stock actual; no dependen del rango) ──────────────────
    public int ProductosActivos { get; set; }
    /// <summary>Valorización al costo: Σ stock actual × precio de costo.</summary>
    public decimal ValorInventario { get; set; }
    /// <summary>Productos con stock ≤ mínimo configurado (mínimo &gt; 0).</summary>
    public int StockCritico { get; set; }
    public int SinStock { get; set; }
    public int UnidadesEnStock { get; set; }

    // ── Movimientos del período (aprobados, por FechaMovimiento) ──────────────────
    public int TotalEntradas { get; set; }
    public int TotalSalidas { get; set; }

    // ── Desgloses ─────────────────────────────────────────────────────────────
    public List<SeriePuntoInventarioDto> SerieTemporal { get; set; } = new();
    public List<CategoriaInventarioDto> PorCategoria { get; set; } = new();
    public List<ProductoCriticoDto> ProductosCriticos { get; set; } = new();
    public List<ProductoValorDto> TopPorValor { get; set; } = new();
}

public class SeriePuntoInventarioDto
{
    public string Periodo { get; set; } = "";
    public int Entradas { get; set; }
    public int Salidas { get; set; }
}

public class CategoriaInventarioDto
{
    public string Categoria { get; set; } = "";
    public int Productos { get; set; }
    public decimal Valor { get; set; }
}

public class ProductoCriticoDto
{
    public string Nombre { get; set; } = "";
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public int Faltante { get; set; }
}

public class ProductoValorDto
{
    public string Nombre { get; set; } = "";
    public int StockActual { get; set; }
    public decimal Valor { get; set; }
}
