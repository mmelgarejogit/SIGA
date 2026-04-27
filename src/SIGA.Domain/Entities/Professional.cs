namespace SIGA.Domain.Entities;

public class Professional
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string LicenseNumber { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ProfesionalEspecialidad> Especialidades { get; set; } = new List<ProfesionalEspecialidad>();
    public ICollection<HorarioProfesional> Horarios { get; set; } = new List<HorarioProfesional>();
    public ICollection<BloqueoFecha> Bloqueos { get; set; } = new List<BloqueoFecha>();
}
