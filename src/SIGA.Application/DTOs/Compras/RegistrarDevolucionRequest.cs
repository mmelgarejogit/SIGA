namespace SIGA.Application.DTOs.Compras;

public class RegistrarDevolucionRequest
{
    public int ItemId { get; set; }
    public int Cantidad { get; set; }
    public string Motivo { get; set; } = "";
}
