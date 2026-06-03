namespace SIGA.Domain.Entities;

public enum ImpactoAjuste { Positivo, Negativo, Ambos }

public class TipoAjuste
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
    public ImpactoAjuste Impacto { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<AjusteManual> AjustesManual { get; set; } = [];
}
