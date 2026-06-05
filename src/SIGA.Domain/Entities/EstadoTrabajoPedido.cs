namespace SIGA.Domain.Entities;

public enum EstadoTrabajoPedido
{
    PendienteAprobacion = 0,
    PendienteEnvio      = 1,
    Enviado             = 2,
    Recibido            = 3,
    Rechazado           = 4,
}
