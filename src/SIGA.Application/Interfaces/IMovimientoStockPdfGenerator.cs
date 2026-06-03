using SIGA.Application.DTOs.Productos;

namespace SIGA.Application.Interfaces;

public interface IMovimientoStockPdfGenerator
{
    byte[] Generate(MovimientoStockResponse movimiento);
}
