namespace SIGA.Domain.Entities;

public class Sucursal
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Codigo { get; set; } = "";
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AjusteManual> AjustesManual { get; set; } = [];
    public ICollection<Transferencia> TransferenciasOrigen { get; set; } = [];
    public ICollection<Transferencia> TransferenciasDestino { get; set; } = [];
    public ICollection<ParametroStock> ParametrosStock { get; set; } = [];
    public ICollection<MovimientoInventario> Movimientos { get; set; } = [];
}
