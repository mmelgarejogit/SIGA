namespace SIGA.Domain.Entities;

public class Cobro
{
    public int Id { get; set; }

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public TipoCobro Tipo { get; set; } = TipoCobro.Cuota;
    public decimal MontoTotal { get; set; }
    public DateOnly Fecha { get; set; }
    public bool Anulado { get; set; } = false;

    public int RegistradoPorId { get; set; }
    public User RegistradoPor { get; set; } = null!;

    public ICollection<CobroLinea> Lineas { get; set; } = new List<CobroLinea>();

    public DateTime CreatedAt { get; set; }
}
