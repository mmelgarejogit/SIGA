namespace SIGA.Application.DTOs.Inventario;

public class UpdateStockConfigRequest
{
    public decimal PrecioCosto { get; set; }
    public int StockMinimo { get; set; }
    public int? StockMaximo { get; set; }
}
