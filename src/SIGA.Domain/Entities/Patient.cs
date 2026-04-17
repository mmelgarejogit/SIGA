namespace SIGA.Domain.Entities;

public class Patient
{
    public int Id { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public int? UserId { get; set; }
    public User? User { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
