namespace SIGA.Application.DTOs.Stock;

public class MovimientoInventarioResponse
{
    public Guid Id { get; set; }
    public Guid ProductoVarianteId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public string? VarianteSku { get; set; }
    public string? VarianteColor { get; set; }
    public string? VarianteTalle { get; set; }
    public Guid SucursalId { get; set; }
    public string SucursalNombre { get; set; } = "";
    public string Tipo { get; set; } = "";
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = "";
    public string OrigenTipo { get; set; } = "";
    public Guid? ReferenciaId { get; set; }
    public string? TipoAjusteNombre { get; set; }
}

public class StockPorVarianteResponse
{
    public Guid ProductoVarianteId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public string? Sku { get; set; }
    public string? Color { get; set; }
    public string? Talle { get; set; }
    public Guid SucursalId { get; set; }
    public string SucursalNombre { get; set; } = "";
    public int StockActual { get; set; }
    public int? StockMinimo { get; set; }
    public int? StockMaximo { get; set; }
    public bool BajoStock => StockMinimo.HasValue && StockActual <= StockMinimo.Value;
}
