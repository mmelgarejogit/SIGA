namespace SIGA.Domain.Entities;

public class Honorario : Egreso
{
    public int ProfessionalId { get; set; }
    public Professional Professional { get; set; } = null!;
    public int PeriodoMes { get; set; }
    public int PeriodoAnio { get; set; }

    public Honorario()
    {
        Tipo = TipoEgreso.Honorario;
    }
}