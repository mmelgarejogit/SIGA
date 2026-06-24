using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Services;

internal static class FacturaMontoCalculator
{
    /// <summary>
    /// Suma los subtotales de los ítems agrupados por TipoIva.
    /// El precio del ítem es el precio bruto (con IVA incluido) según convención PY.
    /// </summary>
    public static (decimal exento, decimal gravado5, decimal gravado10) ComputeFromItems(
        IEnumerable<FacturaCompraItem> items)
    {
        decimal exento   = 0;
        decimal gravado5 = 0;
        decimal gravado10 = 0;

        foreach (var item in items)
        {
            var subtotal = item.Cantidad * item.PrecioUnitario;
            switch (item.TipoIva)
            {
                case TipoIvaFactura.Exento: exento   += subtotal; break;
                case TipoIvaFactura.Iva5:   gravado5 += subtotal; break;
                case TipoIvaFactura.Iva10:  gravado10 += subtotal; break;
            }
        }

        return (exento, gravado5, gravado10);
    }
}
