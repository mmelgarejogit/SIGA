namespace SIGA.Domain.Entities;

public class Honorario : Egreso
{
    public int ProfessionalId { get; set; }
    public Professional Professional { get; set; } = null!;
    public string? Periodo { get; set; }

    public Honorario()
    {
        Tipo = TipoEgreso.Honorario;
    }
}
