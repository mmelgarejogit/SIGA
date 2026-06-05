namespace SIGA.Domain.Entities;

public class Ciudad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int DepartamentoId { get; set; }
    public Departamento Departamento { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
