using SIGA.Application.DTOs.Inventario;

namespace SIGA.Application.Interfaces;

public interface IMovimientoStockPdfGenerator
{
    byte[] Generate(MovimientoStockResponse movimiento);
}
