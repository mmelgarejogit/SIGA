namespace SIGA.Application.DTOs.Stock;

public class ParametroStockResponse
{
    public Guid ProductoVarianteId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public string? VarianteSku { get; set; }
    public string? VarianteColor { get; set; }
    public string? VarianteTalle { get; set; }
    public Guid SucursalId { get; set; }
    public string SucursalNombre { get; set; } = "";
    public int StockMinimo { get; set; }
    public int StockMaximo { get; set; }
}

public class UpsertParametroStockRequest
{
    public Guid ProductoVarianteId { get; set; }
    public Guid SucursalId { get; set; }
    public int StockMinimo { get; set; }
    public int StockMaximo { get; set; }
}
