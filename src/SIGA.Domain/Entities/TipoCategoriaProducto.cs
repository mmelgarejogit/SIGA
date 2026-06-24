namespace SIGA.Domain.Entities;

/// <summary>
/// Rol óptico de una categoría de producto. Determina si sus productos se ofrecen
/// como armazón o como cristal en el flujo de venta a pedido.
/// </summary>
public enum TipoCategoriaProducto
{
    Generico = 0,
    Armazon  = 1,
    /// <summary>
    /// [Obsoleto] Los cristales/lentes graduados ya no se modelan como producto con stock;
    /// son una especificación del trabajo a pedido (diseño + tratamientos + precio). El valor
    /// se conserva por compatibilidad de datos, pero no se ofrece al dar de alta categorías.
    /// </summary>
    Cristal  = 2,
}
