namespace SIGA.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    /// <summary>Sucursal a la que pertenece el usuario. null = usuario global (admin), ve todas.</summary>
    public int? SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }

    public string PasswordHash { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }
    public bool MustChangePassword { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public Professional? Professional { get; set; }
    public Patient? Patient { get; set; }
}
