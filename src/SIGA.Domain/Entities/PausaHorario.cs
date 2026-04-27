namespace SIGA.Domain.Entities;

public class PausaHorario
{
    public int Id { get; set; }

    public int HorarioProfesionalId { get; set; }
    public HorarioProfesional HorarioProfesional { get; set; } = null!;

    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public string? Descripcion { get; set; }
}
