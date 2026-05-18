namespace SIGA.Domain.Entities;

public enum EstadoVenta
{
    Abierta         = 0,
    Confirmada      = 1,
    PendienteDePago = 2,
    Pagada          = 3,
    Cobrada         = 4,
    Anulada         = 5,
}
