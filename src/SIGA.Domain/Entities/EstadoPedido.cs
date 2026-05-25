namespace SIGA.Domain.Entities;

public enum EstadoPedido
{
    Borrador        = 0,
    Confirmada      = 1,  // OC enviada/confirmada al proveedor (era Emitida)
    RecibidaParcial = 2,  // Mercadería recibida parcialmente (era ParcialmenteRecibida)
    RecibidaTotal   = 3,  // Mercadería recibida en su totalidad (era Recibida)
    Cancelada       = 4,
    Facturada       = 5,  // Factura del proveedor registrada
}
