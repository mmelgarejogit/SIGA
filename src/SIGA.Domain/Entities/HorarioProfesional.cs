namespace SIGA.Domain.Entities;

public class HorarioProfesional
{
    public int Id { get; set; }

    public int ProfessionalId { get; set; }
    public Professional Professional { get; set; } = null!;

    public DayOfWeek DiaSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<PausaHorario> Pausas { get; set; } = new List<PausaHorario>();
}
