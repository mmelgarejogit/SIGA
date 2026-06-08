namespace SIGA.Domain.Entities;

/// <summary>
/// Rol óptico de una categoría de producto. Determina si sus productos se ofrecen
/// como armazón o como cristal en el flujo de venta a pedido.
/// </summary>
public enum TipoCategoriaProducto
{
    Generico = 0,
    Armazon  = 1,
    Cristal  = 2,
}
