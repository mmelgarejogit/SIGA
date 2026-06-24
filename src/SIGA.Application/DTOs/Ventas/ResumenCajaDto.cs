namespace SIGA.Application.DTOs.Ventas;

public class ResumenCajaDto
{
    public string Fecha { get; set; } = "";
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal SaldoNeto { get; set; }
    public decimal EfectivoTotal { get; set; }
    public decimal TarjetaTotal { get; set; }
    public decimal TransferenciaTotal { get; set; }
    public decimal ChequeTotal { get; set; }
    public int CantidadVentas { get; set; }
    public List<MovimientoCajaDto> Movimientos { get; set; } = new();
}
