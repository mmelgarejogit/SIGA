namespace SIGA.Domain.Entities;

public enum TipoLineaVenta
{
    Producto = 0,
    Servicio = 1,
    /// <summary>Lente graduado (cristal) hecho a pedido. No descuenta stock; se pide al laboratorio.</summary>
    Lente    = 2,
}
