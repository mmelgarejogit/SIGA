namespace SIGA.Domain.Entities;

public class ParametroStock
{
    public Guid ProductoVarianteId { get; set; }
    public ProductoVariante ProductoVariante { get; set; } = null!;
    public Guid SucursalId { get; set; }
    public Sucursal Sucursal { get; set; } = null!;
    public int StockMinimo { get; set; }
    public int StockMaximo { get; set; }
}
