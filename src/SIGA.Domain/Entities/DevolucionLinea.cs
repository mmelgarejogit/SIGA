namespace SIGA.Domain.Entities;

public class DevolucionLinea
{
    public int Id { get; set; }

    public int DevolucionId { get; set; }
    public Devolucion Devolucion { get; set; } = null!;

    // Producto que el cliente devuelve
    public int ProductoDevueltoId { get; set; }
    public Producto ProductoDevuelto { get; set; } = null!;
    public int CantidadDevuelta { get; set; }

    // Solo para tipo Cambio
    public int? ProductoNuevoId { get; set; }
    public Producto? ProductoNuevo { get; set; }
    public int? CantidadNueva { get; set; }
}
