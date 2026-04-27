namespace SIGA.Domain.Entities;

public class ProfesionalEspecialidad
{
    public int ProfessionalId { get; set; }
    public Professional Professional { get; set; } = null!;

    public int EspecialidadId { get; set; }
    public Especialidad Especialidad { get; set; } = null!;
}
