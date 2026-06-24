namespace SIGA.Domain.Entities;

public class CobroLinea
{
    public int Id { get; set; }

    public int CobroId { get; set; }
    public Cobro Cobro { get; set; } = null!;

    public MetodoPago MetodoPago { get; set; }
    public decimal Monto { get; set; }
}
