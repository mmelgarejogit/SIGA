namespace SIGA.Domain.Entities;

public class GastoGeneral : Egreso
{
    public int CategoriaGastoId { get; set; }
    public CategoriaGasto CategoriaGasto { get; set; } = null!;

    public GastoGeneral()
    {
        Tipo = TipoEgreso.GastoGeneral;
    }
}
