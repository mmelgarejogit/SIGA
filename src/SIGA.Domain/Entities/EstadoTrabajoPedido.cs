namespace SIGA.Domain.Entities;

public enum EstadoTrabajoPedido
{
    PendienteAprobacion = 0,
    PendienteEnvio      = 1,
    Enviado             = 2,
    Recibido            = 3,
    Rechazado           = 4,
    // Config óptica de un presupuesto todavía no confirmado; no entra a la cola del laboratorio.
    Borrador            = 5,
    // El cliente ya pasó a retirar los lentes terminados. Estado terminal del ciclo físico.
    Entregado           = 6,
}
