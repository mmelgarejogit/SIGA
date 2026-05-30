namespace SIGA.Domain.Entities;

public class CargoEmpleado
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Empleado> Empleados { get; set; } = [];
}
