namespace SIGA.Domain.Entities;

public class InventarioFisicoLinea
{
    public Guid Id { get; set; }
    public Guid InventarioFisicoId { get; set; }
    public InventarioFisico InventarioFisico { get; set; } = null!;
    public Guid ProductoVarianteId { get; set; }
    public ProductoVariante ProductoVariante { get; set; } = null!;
    public int CantidadSistema { get; set; }
    public int? CantidadContada { get; set; }
    public int? Diferencia { get; set; }
}
