namespace SIGA.Domain.Entities;

public class Person
{
    public int Id { get; set; }

    public string DNI { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly BirthDate { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
}
