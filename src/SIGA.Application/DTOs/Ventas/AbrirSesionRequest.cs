namespace SIGA.Application.DTOs.Ventas;

public class AbrirSesionRequest
{
    // null = apertura automática: se toma el efectivo del último cierre.
    // Con valor = ajuste manual del fondo inicial.
    public decimal? MontoInicial { get; set; }
}
