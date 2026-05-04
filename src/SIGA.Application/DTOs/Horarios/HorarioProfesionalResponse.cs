namespace SIGA.Application.DTOs.Horarios;

public class HorarioProfesionalResponse
{
    public int Id { get; set; }
    public DayOfWeek DiaSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public bool Activo { get; set; }
    public List<PausaResponse> Pausas { get; set; } = [];
}

public class PausaResponse
{
    public int Id { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public string? Descripcion { get; set; }
}
