namespace SIGA.Application.DTOs.Empleados;

public class EmpleadoResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public string CI { get; set; } = "";
    public int CargoId { get; set; }
    public string CargoNombre { get; set; } = "";
    public string FechaIngreso { get; set; } = "";
    public string? FechaEgreso { get; set; }
    public decimal? SalarioBase { get; set; }
    public int? SucursalId { get; set; }
    public string? SucursalNombre { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
