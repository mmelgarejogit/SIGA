namespace SIGA.Application.DTOs.Horarios;

public class SetHorariosRequest
{
    public List<HorarioDiaRequest> Horarios { get; set; } = [];
}

public class HorarioDiaRequest
{
    public DayOfWeek DiaSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public bool Activo { get; set; } = true;
    public List<PausaRequest> Pausas { get; set; } = [];
}

public class PausaRequest
{
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public string? Descripcion { get; set; }
}
