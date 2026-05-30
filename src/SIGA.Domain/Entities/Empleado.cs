namespace SIGA.Domain.Entities;

public class Empleado
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int CargoId { get; set; }
    public CargoEmpleado Cargo { get; set; } = null!;
    public DateOnly FechaIngreso { get; set; }
    public DateOnly? FechaEgreso { get; set; }
    public decimal? SalarioBase { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
