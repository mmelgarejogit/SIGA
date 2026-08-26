namespace SIGA.Domain.Entities;

public class Receta
{
    public int Id { get; set; }

    // Sucursal donde se emitió (clínica) o donde se cargó (externa/de mostrador). Se usa
    // solo para reportes — a propósito NO tiene filtro global de EF: el flujo de venta a
    // pedido necesita ver las recetas de un cliente en cualquier sucursal (RecetaService.GetByClienteAsync).
    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }

    // Opcional: una receta clínica cuelga de una consulta; una receta externa/de mostrador no.
    public int? ConsultaClinicaId { get; set; }
    public ConsultaClinica? ConsultaClinica { get; set; }

    // Persona dueña de la receta (paciente o cliente). Permite listar todas las recetas
    // de un cliente, sean clínicas o cargadas a mano.
    public int? PersonId { get; set; }
    public Person? Person { get; set; }

    public DateOnly FechaEmision { get; set; }

    public decimal? OdEsferico { get; set; }
    public decimal? OdCilindro { get; set; }
    public int? OdEje { get; set; }
    public decimal? OdAdicion { get; set; }

    public decimal? OiEsferico { get; set; }
    public decimal? OiCilindro { get; set; }
    public int? OiEje { get; set; }
    public decimal? OiAdicion { get; set; }

    public decimal? DistanciaInterpupilar { get; set; }
    public string? AvSinCorreccion { get; set; }
    public string? AvConCorreccion { get; set; }
    public string? Observaciones { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
