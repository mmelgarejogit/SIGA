namespace SIGA.Application.DTOs.Compras;

public class RegistrarRecepcionRequest
{
    public string? Observaciones { get; set; }
    public IEnumerable<RecepcionItemRequest> Items { get; set; } = [];
}

public class RecepcionItemRequest
{
    public int ItemId { get; set; }
    public int CantidadRecibida { get; set; }
}
