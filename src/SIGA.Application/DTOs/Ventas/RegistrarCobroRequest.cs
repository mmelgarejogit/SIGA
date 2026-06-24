namespace SIGA.Application.DTOs.Ventas;

public class CobroLineaRequest
{
    public string MetodoPago { get; set; } = "Efectivo";
    public decimal Monto { get; set; }
}

public class RegistrarCobroRequest
{
    public int VentaId { get; set; }
    public string Tipo { get; set; } = "Cuota";
    public string Fecha { get; set; } = null!;
    public List<CobroLineaRequest> Lineas { get; set; } = new();
}
