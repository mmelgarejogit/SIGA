namespace SIGA.Domain.Entities;

public class ConteoInventarioLinea
{
    public int Id { get; set; }
    public int ConteoId { get; set; }
    public ConteoInventario Conteo { get; set; } = null!;
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int CantidadSistema { get; set; }
    public int CantidadFisica { get; set; }
    public int Diferencia { get; set; }
}
