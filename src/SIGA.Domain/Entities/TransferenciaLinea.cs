namespace SIGA.Domain.Entities;

public class TransferenciaLinea
{
    public Guid Id { get; set; }
    public Guid TransferenciaId { get; set; }
    public Transferencia Transferencia { get; set; } = null!;
    public Guid ProductoVarianteId { get; set; }
    public ProductoVariante ProductoVariante { get; set; } = null!;
    public int Cantidad { get; set; }
}
