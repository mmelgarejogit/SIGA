namespace SIGA.Application.DTOs.Empleados;

public class CrearEmpleadoRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string CI { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public int CargoId { get; set; }
    public string FechaIngreso { get; set; } = "";
    public decimal? SalarioBase { get; set; }
    public int? SucursalId { get; set; }
}
