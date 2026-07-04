namespace SIGA.Domain.Entities;

public class EgresoFacturaLaboratorio : Egreso
{
    public int FacturaLaboratorioId { get; set; }
    public FacturaLaboratorio FacturaLaboratorio { get; set; } = null!;

    public EgresoFacturaLaboratorio()
    {
        Tipo = TipoEgreso.FacturaLaboratorio;
    }
}
