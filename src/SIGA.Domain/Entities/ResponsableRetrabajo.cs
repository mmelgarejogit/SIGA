namespace SIGA.Domain.Entities;

// Quién asume el costo de rehacer el trabajo. El cliente nunca paga un re-trabajo (es garantía):
// si el cliente quiere un producto distinto y paga, eso es una venta nueva, no un re-trabajo.
public enum ResponsableRetrabajo
{
    // Lo rehace el laboratorio sin costo para la óptica (defecto propio del lab).
    Laboratorio = 1,
    // La óptica absorbe el costo (error propio o gesto comercial); si el lab la factura, es un egreso.
    Optica      = 2,
}
