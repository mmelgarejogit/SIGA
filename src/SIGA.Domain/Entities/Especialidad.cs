namespace SIGA.Domain.Entities;

public class Especialidad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }

    public ICollection<ProfesionalEspecialidad> Profesionales { get; set; } = new List<ProfesionalEspecialidad>();
}
