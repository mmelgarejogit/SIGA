namespace SIGA.Application.DTOs.Productos;

public class UpdateStockInfoRequest
{
    public decimal PrecioCosto { get; set; }
    public int StockMinimo { get; set; }
}
