namespace SIGA.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public Professional? Professional { get; set; }
    public Patient? Patient { get; set; }
}
